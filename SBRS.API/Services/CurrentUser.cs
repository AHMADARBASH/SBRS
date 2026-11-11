using System.Security.Claims;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Constants;

namespace SBRS.Services;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public Guid? UserId => ParseGuid(Principal?.FindFirstValue(ClaimTypes.NameIdentifier));
    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);
    public Guid? OrganizationId => ParseGuid(Principal?.FindFirstValue(AppClaims.OrganizationId));
    public Guid? FacilityId => ParseGuid(Principal?.FindFirstValue(AppClaims.FacilityId));
    public bool IsSystemAdmin => Role == Roles.SystemAdmin;

    private static Guid? ParseGuid(string? value)
        => Guid.TryParse(value, out var id) ? id : null;
}