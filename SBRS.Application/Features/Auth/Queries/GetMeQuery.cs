using MediatR;
using SBRS.Application.Common.DTOs;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Auth.DTOs;
using SBRS.Domain.Constants;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Auth.Queries;

/// <summary>
/// One call for the client shell after login: the full user object plus the full
/// facility/organization context (facility type, branch, organization resolved),
/// replacing the previous flat "me" response and saving follow-up requests.
/// </summary>
public sealed record GetMeQuery : IRequest<CurrentUserDto>;

public sealed class GetMeQueryHandler(
    IIdentityService identity,
    IFacilityRepository facilities,
    IFacilityTypeRepository facilityTypes,
    IBranchRepository branches,
    IOrganizationRepository organizations,
    ICurrentUser currentUser)
    : IRequestHandler<GetMeQuery, CurrentUserDto>
{
    public async Task<CurrentUserDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedException("No authenticated user.");

        var user = await identity.GetAuthenticatedUserAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException("The user no longer exists.");

        // OrganizationAdmin: their own organization; facility roles: resolved through the branch below.
        var organization = user.OrganizationId is { } organizationId
            ? MapOrganization(await organizations.GetByIdAsync(organizationId, cancellationToken))
            : null;

        CurrentFacilityDto? facility = null;
        if (user.FacilityId is { } facilityId)
        {
            // scoped repository: facility roles only ever see their own facility
            var facilityEntity = await facilities.GetByIdAsync(facilityId, cancellationToken);
            if (facilityEntity is not null)
            {
                var type = await facilityTypes.GetByIdAsync(facilityEntity.TypeId, cancellationToken)
                    ?? throw new NotFoundException(nameof(FacilityTypeEntity), facilityEntity.TypeId);
                var branch = await branches.GetByIdAsync(facilityEntity.BranchId, cancellationToken)
                    ?? throw new NotFoundException(nameof(BranchEntity), facilityEntity.BranchId);

                // the branch's organization, when the user has none of their own
                organization ??= MapOrganization(await organizations.GetByIdAsync(branch.organizationId, cancellationToken));

                facility = new CurrentFacilityDto(
                    facilityEntity.Id,
                    facilityEntity.ArName,
                    facilityEntity.EnName,
                    facilityEntity.Code,
                    facilityEntity.IsActive,
                    new FacilityTypeSummaryDto(type.Id, type.Code, type.ArName, type.EnName),
                    new BranchSummaryDto(branch.Id, branch.ArName, branch.EnName));
            }
        }

        return new CurrentUserDto(
            user.Id, user.UserName, user.FirstName, user.LastName, user.Role,
            user.Role == Roles.SystemAdmin, user.IsMobile,
            user.OrganizationId, user.FacilityId,
            organization, facility);
    }

    private static OrganizationSummaryDto? MapOrganization(OrganizationEntity? organization) =>
        organization is null
            ? null
            : new OrganizationSummaryDto(
                organization.Id, organization.Code, organization.ArName, organization.EnName, organization.IsActive);
}
