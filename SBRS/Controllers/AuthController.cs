using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Auth.Commands;
using SBRS.Application.Features.Auth.DTOs;
using SBRS.Contracts.Auth;

namespace SBRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(ISender sender, ICurrentUser currentUser) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResultDto>> Login(LoginRequest body, CancellationToken ct)
            => Ok(await sender.Send(new LoginCommand(body.UserName, body.Password), ct));

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
        public async Task<ActionResult<LoginResultDto>> Refresh(RefreshTokenRequest body, CancellationToken ct)
            => Ok(await sender.Send(new RefreshTokenCommand(body.RefreshToken), ct));

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenRequest body, CancellationToken ct)
        {
            await sender.Send(new LogoutCommand(body.RefreshToken), ct);
            return NoContent();
        }

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll(CancellationToken ct)
        {
            await sender.Send(new LogoutAllCommand(), ct);
            return NoContent();
        }
    }
}
