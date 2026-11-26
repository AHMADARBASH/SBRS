using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Models;
using SBRS.Application.Features.Visits.Commands;
using SBRS.Application.Features.Visits.DTOs;
using SBRS.Application.Features.Visits.Queries;
using SBRS.Authorization;
using SBRS.Contracts.Visits;
using SBRS.Domain.Exceptions;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitsController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    private Guid FacilityId => currentUser.FacilityId
        ?? throw new DomainException("No facility is assigned to this user.");

    [Authorize(Policy = Policies.RecordVisits)]
    [HttpPost("Add")]
    public async Task<ActionResult<VisitDto>> Add(AddVisitRequest body, CancellationToken ct)
    {
        var dto = await sender.Send(new AddVisitCommand(
            FacilityId, body.BeneficiaryId, body.VisitDate, body.Weight, body.Height, body.IsPregnant), ct);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [Authorize(Policy = Policies.UpdateVisits)]
    [HttpPut("Update")]
    public async Task<ActionResult<VisitDto>> Update(UpdateVisitRequest body, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateVisitCommand(
            body.Id, FacilityId, body.VisitDate, body.Weight, body.Height, body.IsPregnant), ct));

    [Authorize(Policy = Policies.DeleteVisits)]
    [HttpDelete("Delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteVisitCommand(id, FacilityId), ct);
        return NoContent();
    }

    [Authorize(Policy = Policies.ViewVisits)]
    [HttpGet("GetAll")]
    public async Task<ActionResult<PagedResult<VisitDto>>> GetAll(
        [FromQuery] Guid? beneficiaryId, [FromQuery] int page = 1, CancellationToken ct = default) =>
        Ok(await sender.Send(new GetVisitsQuery(FacilityId, beneficiaryId, page), ct));

    [Authorize(Policy = Policies.ViewVisits)]
    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<VisitDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetVisitByIdQuery(id, FacilityId), ct));
}
