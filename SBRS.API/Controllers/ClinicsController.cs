using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Clinics.Commands;
using SBRS.Application.Features.Clinics.DTOs;
using SBRS.Application.Features.Clinics.Queries;
using SBRS.Authorization;
using SBRS.Contracts.Clinics;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicsController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    [Authorize(Policy = Policies.ViewClinics)]
    [HttpGet("GetAll")]
    public async Task<ActionResult<IReadOnlyList<ClinicDto>>> GetAll(
        [FromQuery] Guid? facilityId, [FromQuery] bool includeInactive = false, CancellationToken ct = default) =>
        Ok(await sender.Send(new GetClinicsQuery(facilityId, includeInactive), ct));

    [Authorize(Policy = Policies.ViewClinics)]
    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<ClinicDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetClinicByIdQuery(id), ct));

    [Authorize(Policy = Policies.ManageClinics)]
    [HttpPost("Add")]
    public async Task<ActionResult<ClinicDto>> Add(AddClinicRequest body, CancellationToken ct)
    {
        var facilityId = currentUser.IsSystemAdmin ? body.FacilityId : currentUser.FacilityId;
        var dto = await sender.Send(new AddClinicCommand(facilityId, body.ClinicTypeId), ct);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [Authorize(Policy = Policies.ManageClinics)]
    [HttpPost("Deactivate/{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeactivateClinicCommand(id), ct);
        return NoContent();
    }

    [Authorize(Policy = Policies.ManageClinics)]
    [HttpPost("Activate/{id:guid}")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await sender.Send(new ActivateClinicCommand(id), ct);
        return NoContent();
    }
}
