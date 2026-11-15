using SBRS.Application.Common.DTOs;

namespace SBRS.Application.Features.Users.DTOs;

/// <summary>
/// One row of the user list. Scope is nested as the shared summary objects
/// (organization / branch / facility) so the admin screen can render names
/// without extra lookups.
/// </summary>
public sealed record UserListItemDto(
    Guid Id,
    string UserName,
    string FirstName,
    string LastName,
    string Role,
    bool IsMobile,
    bool IsActive,
    DateTime CreatedAt,
    OrganizationSummaryDto? Organization,
    BranchSummaryDto? Branch,
    FacilitySummaryDto? Facility);
