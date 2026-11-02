using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.Users.Commands;
using SBRS.Application.Features.Users.DTOs;
using SBRS.Application.Features.Users.Queries;
using SBRS.Authorization;
using SBRS.Contracts.Users;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.SystemAdminOnly)]
public class UsersController(ISender sender) : ControllerBase
{
    // which role to use when creating a user (and which ids that role expects)
    [HttpGet("Roles")]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> Roles(CancellationToken ct) =>
        Ok(await sender.Send(new GetRolesQuery(), ct));

    [HttpPost]
    [Route("CreateUser")]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest r, CancellationToken ct)
    {
        var user = await sender.Send(new CreateUserCommand(
            r.UserName, r.Password, r.FirstName, r.LastName,
            r.Role, r.OrganizationId, r.FacilityId, r.IsMobile), ct);

        return StatusCode(StatusCodes.Status201Created, user);
    }

    [HttpPut()]
    [Route("UpdateUser")]
    public async Task<ActionResult<UserDto>> Update( UpdateUserRequest r, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateUserCommand(
            r.id, r.FirstName, r.LastName, r.Role, r.OrganizationId, r.FacilityId, r.IsMobile), ct));

    [HttpPost("deactivateUser/{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeactivateUserCommand(id), ct);
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword( ResetUserPasswordRequest r, CancellationToken ct)
    {
        await sender.Send(new ResetUserPasswordCommand(r.id, r.NewPassword), ct);
        return NoContent();
    }
}