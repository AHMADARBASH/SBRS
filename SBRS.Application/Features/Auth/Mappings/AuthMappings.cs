using SBRS.Application.Common.Models;
using SBRS.Application.Features.Auth.DTOs;

namespace SBRS.Application.Features.Auth.Mappings;

public static class AuthMappings
{
    /// <summary>Login/Refresh = access token + the Me payload, so both responses match.</summary>
    public static LoginResultDto ToLoginResult(
        this CurrentUserDto user, TokenResult access, string refreshToken)
        => new(access.Token, refreshToken, access.ExpiresAt,
               user.UserId, user.UserName, user.FirstName, user.LastName, user.Role,
               user.IsSystemAdmin, user.IsMobile,
               user.OrganizationId, user.BranchId, user.FacilityId,
               user.Organization, user.Facility);
}
