using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SBRS.Application.Common.DTOs;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Models;
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
        string role, Guid? organizationId, Guid? branchId, Guid? facilityId, bool isMobile,
        CancellationToken ct)
    {
        var (orgId, branch, facId) = await ResolveScopeAsync(role, organizationId, branchId, facilityId, ct);

        if (await userManager.FindByNameAsync(userName) is not null)
            throw new ConflictException($"User name '{userName}' is already taken.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            FirstName = firstName,
            LastName = lastName,
            OrganizationId = orgId,
            BranchId = branch,
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
        string role, Guid? organizationId, Guid? branchId, Guid? facilityId, bool isMobile,
        CancellationToken ct)
    {
        var user = await FindAsync(id);
        var (orgId, branch, facId) = await ResolveScopeAsync(role, organizationId, branchId, facilityId, ct);

        var oldRoles = await userManager.GetRolesAsync(user);
        var roleChanged = oldRoles.Count != 1 || oldRoles[0] != role;
        var scopeChanged = user.OrganizationId != orgId || user.BranchId != branch || user.FacilityId != facId;

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
        user.BranchId = branch;
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

        // Deactivation is refused for the target SystemAdmin account, not the caller.
        if (await userManager.IsInRoleAsync(user, Roles.SystemAdmin))
            throw new DomainException("System Admin cannot be deactivated.");

        user.IsActive = false;
        Check(await userManager.UpdateAsync(user));

        await refreshTokens.RevokeAllForUserAsync(user.Id, ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct)
    {
        var user = await FindAsync(id);
        if (user.IsActive) return;

        user.IsActive = true;
        Check(await userManager.UpdateAsync(user));
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

    public async Task<PagedResult<UserListItemDto>> GetPagedAsync(
        int page, int pageSize, bool includeInactive, string? search, string? role,
        CancellationToken ct)
    {
        var rows =
            from u in db.Users.AsNoTracking()
            join ur in db.UserRoles on u.Id equals ur.UserId into userRoles
            from ur in userRoles.DefaultIfEmpty()
            join r in db.Roles on ur.RoleId equals r.Id into roles
            from r in roles.DefaultIfEmpty()
            select new { User = u, RoleName = r == null ? null : r.Name };

        if (!includeInactive)
            rows = rows.Where(x => x.User.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            rows = rows.Where(x =>
                x.User.UserName!.ToLower().Contains(term) ||
                x.User.FirstName.ToLower().Contains(term) ||
                x.User.LastName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(role))
            rows = rows.Where(x => x.RoleName == role);

        var totalCount = await rows.CountAsync(ct);

        var pageRows = await rows
            .OrderByDescending(x => x.User.CreatedAt)
            .ThenBy(x => x.User.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // one query each for the whole page, instead of per-row lookups
        var organizationIds = pageRows
            .Where(x => x.User.OrganizationId is not null)
            .Select(x => x.User.OrganizationId!.Value)
            .Distinct()
            .ToList();

        var organizations = organizationIds.Count == 0
            ? new Dictionary<Guid, OrganizationSummaryDto>()
            : (await db.Organizations.AsNoTracking()
                    .Where(o => organizationIds.Contains(o.Id))
                    .Select(o => new OrganizationSummaryDto(o.Id, o.Code, o.ArName, o.EnName, o.IsActive))
                    .ToListAsync(ct))
                .ToDictionary(o => o.Id);

        var facilityIds = pageRows
            .Where(x => x.User.FacilityId is not null)
            .Select(x => x.User.FacilityId!.Value)
            .Distinct()
            .ToList();

        var facilities = facilityIds.Count == 0
            ? new Dictionary<Guid, FacilitySummaryDto>()
            : (await db.Facilities.AsNoTracking()
                    .Where(f => facilityIds.Contains(f.Id))
                    .Select(f => new FacilitySummaryDto(f.Id, f.Code, f.ArName, f.EnName, f.IsActive))
                    .ToListAsync(ct))
                .ToDictionary(f => f.Id);

        var branchIds = pageRows
            .Where(x => x.User.BranchId is not null)
            .Select(x => x.User.BranchId!.Value)
            .Distinct()
            .ToList();

        var branches = branchIds.Count == 0
            ? new Dictionary<Guid, BranchSummaryDto>()
            : (await db.Branches.AsNoTracking()
                    .Where(b => branchIds.Contains(b.Id))
                    .Select(b => new BranchSummaryDto(b.Id, b.Code, b.ArName, b.EnName, b.IsActive))
                    .ToListAsync(ct))
                .ToDictionary(b => b.Id);

        var items = pageRows.Select(x => new UserListItemDto(
                x.User.Id,
                x.User.UserName!,
                x.User.FirstName,
                x.User.LastName,
                x.RoleName ?? string.Empty,
                x.User.IsMobile,
                x.User.IsActive,
                x.User.CreatedAt,
                x.User.OrganizationId is { } organizationId && organizations.TryGetValue(organizationId, out var organization)
                    ? organization
                    : null,
                x.User.BranchId is { } branchId && branches.TryGetValue(branchId, out var branch)
                    ? branch
                    : null,
                x.User.FacilityId is { } facilityId && facilities.TryGetValue(facilityId, out var facility)
                    ? facility
                    : null))
            .ToList();

        return new PagedResult<UserListItemDto>(items, page, pageSize, totalCount);
    }

    // ---------- helpers ----------

    private async Task<ApplicationUser> FindAsync(Guid id) =>
        await userManager.FindByIdAsync(id.ToString())
        ?? throw new NotFoundException(nameof(ApplicationUser), id.ToString());

    /// <summary>
    /// Validates the role's scope and returns the OrganizationId/BranchId/FacilityId to store.
    /// A BranchAdmin's organization is always derived from the branch; a facility-level user's
    /// organization is derived from the facility's branch.
    /// </summary>
    private async Task<(Guid? OrganizationId, Guid? BranchId, Guid? FacilityId)> ResolveScopeAsync(
        string role, Guid? organizationId, Guid? branchId, Guid? facilityId, CancellationToken ct)
    {
        switch (role)
        {
            case Roles.SystemAdmin:
                return (null, null, null);

            case Roles.OrganizationAdmin:
                {
                    if (organizationId is null)
                        throw new DomainException("OrganizationId is required for this role.");

                    var org = await db.Set<OrganizationEntity>().AsNoTracking()
                        .Where(o => o.Id == organizationId)
                        .Select(o => new { o.IsActive })
                        .FirstOrDefaultAsync(ct);

                    if (org is null) throw new NotFoundException(nameof(OrganizationEntity), organizationId.Value.ToString());
                    if (!org.IsActive) throw new DomainException("The organization is inactive.");

                    return (organizationId, null, null);
                }

            case Roles.BranchAdmin:
                {
                    if (branchId is null)
                        throw new DomainException("BranchId is required for this role.");

                    var branch = await db.Set<BranchEntity>().AsNoTracking()
                        .Where(b => b.Id == branchId)
                        .Select(b => new { b.IsActive, b.organizationId })
                        .FirstOrDefaultAsync(ct);

                    if (branch is null) throw new NotFoundException(nameof(BranchEntity), branchId.Value.ToString());
                    if (!branch.IsActive) throw new DomainException("The branch is inactive.");

                    // organization is derived from the branch, never trusted from the client
                    return (branch.organizationId, branchId, null);
                }

            case Roles.FacilityAdmin:
            case Roles.DataEntry:
                {
                    if (facilityId is null)
                        throw new DomainException("FacilityId is required for this role.");

                    var facility = await db.Set<FacilityEntity>().AsNoTracking()
                        .Where(f => f.Id == facilityId)
                        .Select(f => new { f.IsActive, f.BranchId, f.Branch.organizationId })
                        .FirstOrDefaultAsync(ct);

                    if (facility is null) throw new NotFoundException(nameof(FacilityEntity), facilityId.Value.ToString());
                    if (!facility.IsActive) throw new DomainException("The facility is inactive.");

                    // Branch and organization are derived from the facility, never trusted from
                    // the client: a facility-level user always lives in the facility's branch.
                    return (facility.organizationId, facility.BranchId, facilityId);
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
        u.OrganizationId, u.BranchId, u.FacilityId, u.IsMobile, u.IsActive, u.CreatedAt);
}