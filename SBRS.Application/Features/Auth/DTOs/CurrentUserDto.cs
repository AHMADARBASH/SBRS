namespace SBRS.Application.Features.Auth.DTOs;

/// <summary>
/// The full session object: the complete user plus the complete facility/organization
/// context in one call, so the client shell needs no follow-up requests after login.
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
    CurrentOrganizationDto? Organization,
    CurrentFacilityDto? Facility);

/// <summary>The caller's own organization (OrganizationAdmin), or the organization owning their facility.</summary>
public sealed record CurrentOrganizationDto(Guid Id, string Code, string ArName, string EnName, bool IsActive);

/// <summary>
/// The caller's facility with its type, branch and owning organization resolved.
/// TypeCode is the stable routing key for the client (facility_types.code).
/// </summary>
public sealed record CurrentFacilityDto(
    Guid Id,
    string ArName,
    string EnName,
    string Code,
    bool IsActive,
    Guid TypeId,
    string TypeCode,
    string TypeArName,
    string TypeEnName,
    Guid BranchId,
    string BranchArName,
    string BranchEnName,
    Guid OrganizationId,
    string OrganizationArName,
    string OrganizationEnName);
