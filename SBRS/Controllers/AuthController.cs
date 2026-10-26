using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Auth.Commands;
using SBRS.Application.Features.Auth.DTOs;
using SBRS.Contracts.Auth;
using SBRS.Cookies;
using SBRS.Services;

namespace SBRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        ISender sender,
        ICurrentUser currentUser,
        IClientTypeResolver clientType,
        IRefreshCookieService cookies) : ControllerBase
    {
        // Web clients send "X-Client-Type: web" and get the refresh token as an
        // HttpOnly cookie; all other clients (mobile/unknown) keep the token in the body.

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResultDto>> Login(LoginRequest body, CancellationToken ct)
        {
            var result = await sender.Send(new LoginCommand(body.UserName, body.Password), ct);

            if (!clientType.IsWeb)
                return Ok(result);   // mobile: unchanged, refresh token in the body

            // web: the refresh token goes to the cookie, never in the response body
            cookies.Set(result.RefreshToken!);
            return Ok(result with { RefreshToken = null });
        }

        // temporary endpoint to verify tokens and the claims read by ICurrentUser
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me() => Ok(new
        {
            currentUser.UserId,
            currentUser.Role,
            currentUser.OrganizationId,
            currentUser.FacilityId,
            currentUser.IsSystemAdmin
        });

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResultDto>> Refresh(RefreshTokenRequest? body, CancellationToken ct)
        {
            if (!clientType.IsWeb)
                return Ok(await sender.Send(
                    new RefreshTokenCommand(body?.RefreshToken ?? string.Empty), ct));   // mobile: unchanged (empty -> validation 400)

            // web: the token comes from the cookie, the body is ignored
            var token = cookies.GetRefreshToken();
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedException("Refresh token is missing.");   // handled as 401

            try
            {
                var result = await sender.Send(new RefreshTokenCommand(token), ct);

                // the NEW rotated refresh token replaces the cookie value
                cookies.Set(result.RefreshToken!);
                return Ok(result with { RefreshToken = null });
            }
            catch (UnauthorizedException)
            {
                // invalid, expired, reused (family revoked) or user gone: the cookie is worthless now
                cookies.Delete();
                throw;
            }
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenRequest? body, CancellationToken ct)
        {
            if (clientType.IsWeb)
            {
                // web: the token is in the cookie; still a success when the cookie is missing
                var token = cookies.GetRefreshToken();
                if (!string.IsNullOrEmpty(token))
                    await sender.Send(new LogoutCommand(token), ct);

                cookies.Delete();
                return NoContent();
            }

            await sender.Send(new LogoutCommand(body?.RefreshToken ?? string.Empty), ct);   // mobile: unchanged (empty -> validation 400)
            return NoContent();
        }

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll(CancellationToken ct)
        {
            await sender.Send(new LogoutAllCommand(), ct);

            if (clientType.IsWeb)
                cookies.Delete();

            return NoContent();
        }
    }
}
