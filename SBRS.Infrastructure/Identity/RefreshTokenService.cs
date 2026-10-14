using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Models;
using SBRS.Infrastructure.Persistence;

namespace SBRS.Infrastructure.Identity;

public class RefreshTokenService(AppDbContext db, IOptions<JwtSettings> options) : IRefreshTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public async Task<string> IssueAsync(Guid userId, CancellationToken ct = default)
    {
        var (raw, entity) = Create(userId, Guid.NewGuid());
        db.RefreshTokens.Add(entity);
        await db.SaveChangesAsync(ct);
        return raw;
    }

    public async Task<RefreshTokenRotation> RotateAsync(string rawToken, CancellationToken ct = default)
    {
        var hash = Hash(rawToken);

        var current = await db.RefreshTokens.AsNoTracking()
                          .FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
                      ?? throw new UnauthorizedException("Invalid refresh token.");

        var now = DateTime.UtcNow;

        if (current.RevokedAt is not null || (current.ExpiresAt is not null && current.ExpiresAt <= now))
            throw new UnauthorizedException("Invalid refresh token.");

        // Atomic single-use claim: only one request can change UsedAt from null.
        var claimed = await db.RefreshTokens
            .Where(t => t.Id == current.Id && t.UsedAt == null && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, now), ct);

        if (claimed == 0)
        {
            // The token was already used: treat it as theft and kill the whole family.
            await RevokeFamilyAsync(current.FamilyId, ct);
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var (raw, next) = Create(current.UserId, current.FamilyId);
        db.RefreshTokens.Add(next);
        await db.SaveChangesAsync(ct);

        return new RefreshTokenRotation(current.UserId, raw);
    }

    public async Task RevokeAsync(string rawToken, CancellationToken ct = default)
    {
        var hash = Hash(rawToken);

        var familyId = await db.RefreshTokens
            .Where(t => t.TokenHash == hash)
            .Select(t => (Guid?)t.FamilyId)
            .FirstOrDefaultAsync(ct);

        if (familyId is { } id)
            await RevokeFamilyAsync(id, ct);
    }

    public Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
        => db.RefreshTokens
             .Where(t => t.UserId == userId && t.RevokedAt == null)
             .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow), ct);

    private Task RevokeFamilyAsync(Guid familyId, CancellationToken ct)
        => db.RefreshTokens
             .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
             .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow), ct);

    private (string Raw, RefreshToken Entity) Create(Guid userId, Guid familyId)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var now = DateTime.UtcNow;

        var entity = new RefreshToken
        {
            UserId = userId,
            FamilyId = familyId,
            TokenHash = Hash(raw),
            CreatedAt = now,
            ExpiresAt = now.AddDays(_settings.RefreshTokenExpiryDays)
        };

        return (raw, entity);
    }

    private static string Hash(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}