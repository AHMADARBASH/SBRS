using MediatR;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.Organizations.Commands;
using SBRS.Application.Features.Organizations.Commands.DeactivateOrganization;
using SBRS.Application.Features.Organizations.Queries.GetOrganizationById;
using SBRS.Application.Features.Organizations.Queries.GetOrganizations;
using SBRS.Contracts.Organizations;

namespace SBRS.API.Controllers;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController(ISender sender) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive, CancellationToken ct)
        => Ok(await sender.Send(new GetOrganizationsQuery(includeInactive), ct));

    [HttpGet("GetById/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetOrganizationByIdQuery(id), ct));

    [HttpPost("Add")]
    public async Task<IActionResult> Add(AddOrganizationRequest body, CancellationToken ct)
    {
        var dto = await sender.Send(new AddOrganizationCommand(body.ArName, body.EnName, body.Code), ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("Update/{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, UpdateOrganizationRequest body, CancellationToken ct)
        => Ok(await sender.Send(new UpdateOrganizationCommand(id, body.ArName, body.EnName, body.Code), ct));

    [HttpPost("deactivate/{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeactivateOrganizationCommand(id), ct);
        return NoContent();
    }
}