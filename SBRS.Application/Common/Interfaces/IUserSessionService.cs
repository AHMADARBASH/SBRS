using SBRS.Application.Common.Models;
using SBRS.Application.Features.Auth.DTOs;

namespace SBRS.Application.Common.Interfaces;

/// <summary>
/// Builds the session payload shared by Me, Login and Refresh. It uses the user's own
/// scope ids (never request input), so it also works during Login, before a token exists.
/// </summary>
public interface IUserSessionService
{
    Task<CurrentUserDto> BuildAsync(AuthenticatedUser user, CancellationToken ct = default);
}
