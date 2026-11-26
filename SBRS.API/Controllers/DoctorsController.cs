using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Doctors.Commands;
using SBRS.Application.Features.Doctors.DTOs;
using SBRS.Application.Features.Doctors.Queries;
using SBRS.Authorization;
using SBRS.Contracts.Doctors;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    [Authorize(Policy = Policies.ViewDoctors)]
    [HttpGet("GetAll")]
    public async Task<ActionResult<IReadOnlyList<DoctorDto>>> GetAll(
        [FromQuery] Guid? facilityId, [FromQuery] Guid? clinicId,
        [FromQuery] bool includeInactive = false, CancellationToken ct = default) =>
        Ok(await sender.Send(new GetDoctorsQuery(facilityId, clinicId, includeInactive), ct));

    [Authorize(Policy = Policies.ViewDoctors)]
    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<DoctorDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetDoctorByIdQuery(id), ct));

    [Authorize(Policy = Policies.ManageDoctors)]
    [HttpPost("Add")]
    public async Task<ActionResult<DoctorDto>> Add(AddDoctorRequest body, CancellationToken ct)
    {
        var facilityId = currentUser.IsSystemAdmin ? body.FacilityId : currentUser.FacilityId;
        var dto = await sender.Send(new AddDoctorCommand(facilityId, body.FirstName, body.LastName), ct);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [Authorize(Policy = Policies.ManageDoctors)]
    [HttpPut("Update")]
    public async Task<ActionResult<DoctorDto>> Update(UpdateDoctorRequest body, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateDoctorCommand(body.Id, body.FirstName, body.LastName), ct));

    [Authorize(Policy = Policies.ManageDoctors)]
    [HttpPost("Deactivate/{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeactivateDoctorCommand(id), ct);
        return NoContent();
    }

    [Authorize(Policy = Policies.ManageDoctors)]
    [HttpPost("Activate/{id:guid}")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await sender.Send(new ActivateDoctorCommand(id), ct);
        return NoContent();
    }

    [Authorize(Policy = Policies.ManageDoctors)]
    [HttpPost("AssignClinic")]
    public async Task<IActionResult> AssignClinic(AssignDoctorClinicRequest body, CancellationToken ct)
    {
        await sender.Send(new AssignClinicCommand(body.DoctorId, body.ClinicId), ct);
        return NoContent();
    }

    [Authorize(Policy = Policies.ManageDoctors)]
    [HttpPost("UnassignClinic")]
    public async Task<IActionResult> UnassignClinic(AssignDoctorClinicRequest body, CancellationToken ct)
    {
        await sender.Send(new UnassignClinicCommand(body.DoctorId, body.ClinicId), ct);
        return NoContent();
    }
}
