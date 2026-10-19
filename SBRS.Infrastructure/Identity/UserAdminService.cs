using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Users.DTOs;
using SBRS.Domain.Constants;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Infrastructure.Persistence;

namespace SBRS.Infrastructure.Identity;

public class UserAdminService(
    UserManager<ApplicationUser> userManager,
    AppDbContext db,
    IRefreshTokenService refreshTokens,
    ICurrentUser currentUser) : IUserAdminService
{
    public async Task<UserDto> CreateAsync(
        string userName, string password, string firstName, string lastName,
        string role, Guid? organizationId, Guid? facilityId, bool isMobile,
        CancellationToken ct)
    {
        var (orgId, facId) = await ResolveScopeAsync(role, organizationId, facilityId, ct);

        if (await userManager.FindByNameAsync(userName) is not null)
            throw new ConflictException($"User name '{userName}' is already taken.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            FirstName = firstName,
            LastName = lastName,
            OrganizationId = orgId,
            FacilityId = facId,
            IsMobile = isMobile,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId
        };

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        Check(await userManager.CreateAsync(user, password));
        Check(await userManager.AddToRoleAsync(user, role));
        await tx.CommitAsync(ct);

        return ToDto(user, role);
    }

    public async Task<UserDto> UpdateAsync(
        Guid id, string firstName, string lastName,
        string role, Guid? organizationId, Guid? facilityId, bool isMobile,
        CancellationToken ct)
    {
        var user = await FindAsync(id);
        var (orgId, facId) = await ResolveScopeAsync(role, organizationId, facilityId, ct);

        var oldRoles = await userManager.GetRolesAsync(user);
        var roleChanged = oldRoles.Count != 1 || oldRoles[0] != role;
        var scopeChanged = user.OrganizationId != orgId || user.FacilityId != facId;

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        if (roleChanged)
        {
            if (oldRoles.Count > 0)
                Check(await userManager.RemoveFromRolesAsync(user, oldRoles));
            Check(await userManager.AddToRoleAsync(user, role));
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.OrganizationId = orgId;
        user.FacilityId = facId;
        user.IsMobile = isMobile;
        Check(await userManager.UpdateAsync(user));

        await tx.CommitAsync(ct);

        // A changed role or scope must not survive on old sessions.
        if (roleChanged || scopeChanged)
            await refreshTokens.RevokeAllForUserAsync(user.Id, ct);

        return ToDto(user, role);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct)
    {
        var user = await FindAsync(id);
        if (!user.IsActive) return;

        user.IsActive = false;
        Check(await userManager.UpdateAsync(user));

        await refreshTokens.RevokeAllForUserAsync(user.Id, ct);
    }

    public async Task ResetPasswordAsync(Guid id, string newPassword, CancellationToken ct)
    {
        var user = await FindAsync(id);

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        Check(await userManager.ResetPasswordAsync(user, token, newPassword));

        // Also unlock the account if it was locked by failed attempts.
        await userManager.ResetAccessFailedCountAsync(user);
        await userManager.SetLockoutEndDateAsync(user, null);

        await refreshTokens.RevokeAllForUserAsync(user.Id, ct);
    }

    // ---------- helpers ----------

    private async Task<ApplicationUser> FindAsync(Guid id) =>
        await userManager.FindByIdAsync(id.ToString())
        ?? throw new NotFoundException(nameof(ApplicationUser), id.ToString());

    /// <summary>
    /// Validates the role's scope and returns the OrganizationId/FacilityId to store.
    /// OrganizationId for facility-level users is derived from the facility's branch.
    /// </summary>
    private async Task<(Guid? OrganizationId, Guid? FacilityId)> ResolveScopeAsync(
        string role, Guid? organizationId, Guid? facilityId, CancellationToken ct)
    {
        switch (role)
        {
            case Roles.SystemAdmin:
                return (null, null);

            case Roles.OrganizationAdmin:
                {
                    var org = await db.Set<OrganizationEntity>().AsNoTracking()
                        .Where(o => o.Id == organizationId)
                        .Select(o => new { o.IsActive })
                        .FirstOrDefaultAsync(ct);

                    if (org is null) throw new NotFoundException(nameof(OrganizationEntity), "");
                    if (!org.IsActive) throw new DomainException("The organization is inactive.");

                    return (organizationId, null);
                }

            case Roles.FacilityAdmin:
            case Roles.DataEntry:
                {
                    var facility = await db.Set<FacilityEntity>().AsNoTracking()
                        .Where(f => f.Id == facilityId)
                        .Select(f => new { f.IsActive, f.Branch.organizationId })
                        .FirstOrDefaultAsync(ct);

                    if (facility is null) throw new NotFoundException(nameof(facility),"");
                    if (!facility.IsActive) throw new DomainException("The facility is inactive.");

                    return (facility.organizationId, facilityId);
                }

            default:
                throw new DomainException(
                    $"Users with the role '{role}' cannot be managed yet.");
        }
    }

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new DomainException(string.Join(" ", result.Errors.Select(e => e.Description)));
    }

    private static UserDto ToDto(ApplicationUser u, string role) => new(
        u.Id, u.UserName!, u.FirstName, u.LastName, role,
        u.OrganizationId, u.FacilityId, u.IsMobile, u.IsActive, u.CreatedAt);
}