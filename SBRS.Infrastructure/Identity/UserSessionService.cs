using Microsoft.EntityFrameworkCore;
using SBRS.Application.Common.DTOs;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Models;
using SBRS.Application.Features.Auth.DTOs;
using SBRS.Domain.Constants;
using SBRS.Domain.Entities;
using SBRS.Infrastructure.Persistence;

namespace SBRS.Infrastructure.Identity;

/// <summary>
/// Composes the session payload returned by Me, Login and Refresh. The scope is taken from the
/// user's own record (OrganizationId/BranchId/FacilityId), not from the request principal, so the
/// same result is produced before authentication (Login) and after it (Me).
/// </summary>
public class UserSessionService(AppDbContext db) : IUserSessionService
{
    public async Task<CurrentUserDto> BuildAsync(AuthenticatedUser user, CancellationToken ct = default)
    {
        // OrganizationAdmin: their own organization. Facility roles fall back to the
        // organization of the facility's branch below.
        var organization = user.OrganizationId is { } organizationId
            ? MapOrganization(await db.Organizations.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == organizationId, ct))
            : null;

        CurrentFacilityDto? facility = null;
        if (user.FacilityId is { } facilityId)
        {
            var facilityEntity = await db.Facilities.AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == facilityId, ct);

            if (facilityEntity is not null)
            {
                var type = await db.FacilityTypes.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == facilityEntity.TypeId, ct)
                    ?? throw new NotFoundException(nameof(FacilityTypeEntity), facilityEntity.TypeId);

                var branch = await db.Branches.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == facilityEntity.BranchId, ct)
                    ?? throw new NotFoundException(nameof(BranchEntity), facilityEntity.BranchId);

                // the branch's organization, when the user has none of their own
                organization ??= MapOrganization(await db.Organizations.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == branch.organizationId, ct));

                facility = new CurrentFacilityDto(
                    facilityEntity.Id,
                    facilityEntity.ArName,
                    facilityEntity.EnName,
                    facilityEntity.Code,
                    facilityEntity.IsActive,
                    new FacilityTypeSummaryDto(type.Id, type.Code, type.ArName, type.EnName),
                    new BranchSummaryDto(branch.Id, branch.Code, branch.ArName, branch.EnName, branch.IsActive));
            }
        }

        return new CurrentUserDto(
            user.Id, user.UserName, user.FirstName, user.LastName, user.Role,
            user.Role == Roles.SystemAdmin, user.IsMobile,
            user.OrganizationId, user.BranchId, user.FacilityId,
            organization, facility);
    }

    private static OrganizationSummaryDto? MapOrganization(OrganizationEntity? organization) =>
        organization is null
            ? null
            : new OrganizationSummaryDto(
                organization.Id, organization.Code, organization.ArName, organization.EnName, organization.IsActive);
}
