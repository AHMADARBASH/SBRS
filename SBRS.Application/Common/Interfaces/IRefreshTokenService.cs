using SBRS.Application.Common.Models;

namespace SBRS.Application.Common.Interfaces;

public interface IRefreshTokenService
{
    /// Starts a new token family (used at login). Returns the raw token to send to the client.
    Task<string> IssueAsync(Guid userId, CancellationToken ct = default);

    /// Burns the given token and returns a new one in the same family.
    /// Throws UnauthorizedException when the token is unknown, revoked, expired, or already used.
    Task<RefreshTokenRotation> RotateAsync(string rawToken, CancellationToken ct = default);

    /// Revokes the whole family of this token (logout on one device). Silent if unknown.
    Task RevokeAsync(string rawToken, CancellationToken ct = default);

    /// Revokes every refresh token of the user (logout everywhere, deactivation).
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}