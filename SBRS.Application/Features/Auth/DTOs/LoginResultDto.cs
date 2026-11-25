using SBRS.Application.Common.DTOs;

namespace SBRS.Application.Features.Auth.DTOs;

/// <summary>
/// Login/Refresh response: the access token plus exactly the same user/session payload as Me
/// (field names match <see cref="CurrentUserDto"/>), so the client shell needs no follow-up call.
/// </summary>
public record LoginResultDto(
    string Token,
    string? RefreshToken,        // omitted (null) for web clients; the token is in an HttpOnly cookie instead
    DateTime ExpiresAt,          // expiry of the access token
    Guid UserId,
    string UserName,
    string FirstName,
    string LastName,
    string Role,
    bool IsSystemAdmin,
    bool IsMobile,
    Guid? OrganizationId,
    Guid? BranchId,
    Guid? FacilityId,
    OrganizationSummaryDto? Organization,
    CurrentFacilityDto? Facility);
