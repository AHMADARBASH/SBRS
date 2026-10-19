using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.Facilities.Commands;
using SBRS.Application.Features.Facilities.DTOs;
using SBRS.Application.Features.Facilities.Queries;
using SBRS.Authorization;
using SBRS.Contracts.Facilities;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacilitiesController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet("GetByBranchId/{branchId:guid}")]
    public async Task<ActionResult<List<FacilityDto>>> GetByBranch(
        Guid branchId, [FromQuery] bool includeInactive, CancellationToken ct)
        => Ok(await sender.Send(new GetFacilitiesByBranchQuery(branchId, includeInactive), ct));

    [Authorize]
    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<FacilityDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetFacilityByIdQuery(id), ct));

    [Authorize(Policy = Policies.OrganizationLevel)]
    [HttpPost("AddFacility")]
    public async Task<ActionResult<FacilityDto>> Add(AddFacilityRequest body, CancellationToken ct)
    {
        var dto = await sender.Send(new AddFacilityCommand(body.branchId,body.addressId,body.typeId,body.partnerId,body.ProjectCode,body.StartDate, body.ArName, body.EnName, body.Code), ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [Authorize(Policy = Policies.OrganizationLevel)]
    [HttpPut("UpdateFacility/{id:guid}")]
    public async Task<ActionResult<FacilityDto>> Update(
        Guid id, UpdateFacilityRequest body, CancellationToken ct)
        => Ok(await sender.Send(new UpdateFacilityCommand(id,body.addressId,body.typeId, body.ArName, body.EnName, body.Code), ct));

    [Authorize(Policy = Policies.OrganizationLevel)]
    [HttpPost("deactivateFacility/{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeactivateFacilityCommand(id), ct);
        return NoContent();
    }
}