using SBRS.Application.Common.DTOs;

namespace SBRS.Application.Features.Auth.DTOs;

/// <summary>
/// The full session object: the complete user plus the complete facility/organization
/// context in one call, so the client shell needs no follow-up requests after login.
/// Scope data is nested (organization / facility.type / facility.branch) so the same
/// summary objects can be reused by other endpoints and extended without re-flattening.
/// </summary>
public sealed record CurrentUserDto(
    Guid UserId,
    string UserName,
    string FirstName,
    string LastName,
    string Role,
    bool IsSystemAdmin,
    bool IsMobile,
    Guid? OrganizationId,
    Guid? FacilityId,
    OrganizationSummaryDto? Organization,
    CurrentFacilityDto? Facility);

/// <summary>
/// The caller's facility with its type and branch nested.
/// <c>Facility.Type.Code</c> is the stable routing key for the client (facility_types.code).
/// </summary>
public sealed record CurrentFacilityDto(
    Guid Id,
    string ArName,
    string EnName,
    string Code,
    bool IsActive,
    FacilityTypeSummaryDto Type,
    BranchSummaryDto Branch);
