using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Models;
using SBRS.Application.Features.BeneficiaryConsultations.Commands;
using SBRS.Application.Features.BeneficiaryConsultations.DTOs;
using SBRS.Application.Features.BeneficiaryConsultations.Queries;
using SBRS.Authorization;
using SBRS.Contracts.BeneficiaryConsultations;
using SBRS.Domain.Exceptions;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BeneficiaryConsultationsController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    private Guid FacilityId => currentUser.FacilityId
        ?? throw new DomainException("No facility is assigned to this user.");

    [Authorize(Policy = Policies.RecordConsultations)]
    [HttpPost("Add")]
    public async Task<ActionResult<BeneficiaryConsultationDto>> Add(AddBeneficiaryConsultationRequest body,
        CancellationToken ct)
    {
        var dto = await sender.Send(new AddBeneficiaryConsultationCommand(
            FacilityId, body.ConsultationDate, body.VisitId, body.ClinicId, body.DoctorId), ct);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [Authorize(Policy = Policies.UpdateConsultations)]
    [HttpPut("Update")]
    public async Task<ActionResult<BeneficiaryConsultationDto>> Update(UpdateBeneficiaryConsultationRequest body,
        CancellationToken ct) =>
        Ok(await sender.Send(new UpdateBeneficiaryConsultationCommand(
            body.Id, FacilityId, body.ConsultationDate, body.ClinicId, body.DoctorId), ct));

    [Authorize(Policy = Policies.DeleteConsultations)]
    [HttpDelete("Delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteBeneficiaryConsultationCommand(id, FacilityId), ct);
        return NoContent();
    }

    [Authorize(Policy = Policies.ViewConsultations)]
    [HttpGet("GetAll")]
    public async Task<ActionResult<PagedResult<BeneficiaryConsultationDto>>> GetAll(
        [FromQuery] Guid? visitId, [FromQuery] Guid? clinicId, [FromQuery] Guid? doctorId,
        [FromQuery] int page = 1, CancellationToken ct = default) =>
        Ok(await sender.Send(new GetBeneficiaryConsultationsQuery(
            FacilityId, visitId, clinicId, doctorId, page), ct));

    [Authorize(Policy = Policies.ViewConsultations)]
    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<BeneficiaryConsultationDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetBeneficiaryConsultationByIdQuery(id, FacilityId), ct));

    [Authorize(Policy = Policies.ViewConsultations)]
    [HttpGet("GetByVisitId/{visitId:guid}")]
    public async Task<ActionResult<IReadOnlyList<BeneficiaryConsultationDto>>> GetByVisitId(
        Guid visitId, CancellationToken ct) =>
        Ok(await sender.Send(new GetBeneficiaryConsultationsByVisitIdQuery(visitId, FacilityId), ct));
}
