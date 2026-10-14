using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Features.Auth.DTOs;

public record LoginResultDto(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,          // expiry of the access token
    Guid UserId,
    string UserName,
    string FirstName,
    string LastName,
    string Role,
    Guid? OrganizationId,
    Guid? FacilityId,
    bool IsMobile);