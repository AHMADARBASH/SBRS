using SBRS.Application.Common.Models;
using SBRS.Application.Features.Auth.DTOs;

namespace SBRS.Application.Features.Auth.Mappings;

public static class AuthMappings
{
    public static LoginResultDto ToLoginResult(
        this AuthenticatedUser u, TokenResult access, string refreshToken)
        => new(access.Token, refreshToken, access.ExpiresAt,
               u.Id, u.UserName, u.FirstName, u.LastName, u.Role,
               u.OrganizationId, u.FacilityId, u.IsMobile);
}