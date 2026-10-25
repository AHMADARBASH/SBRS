using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Common.Models;
using SBRS.Application.Features.Beneficiaries.Commands;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Queries;
using SBRS.Authorization;
using SBRS.Contracts.Beneficiaries;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BeneficiariesController(ISender sender) : ControllerBase
{
    // POST on purpose: names and birth dates must never appear in URLs or logs.
    [HttpPost]
    [Route("Search")]
    [Authorize(Policy = Policies.SearchBeneficiaries)]
    public async Task<ActionResult<BeneficiarySearchResultDto>> Search(SearchBeneficiaryRequest request, CancellationToken ct) =>
        Ok(await sender.Send(new SearchBeneficiaryQuery(request.FullCode, request.FirstName, request.LastName), ct));

    [HttpPost]
    [Route("Register")]
    [Authorize(Policy = Policies.RegisterBeneficiaries)]
    public async Task<ActionResult<RegisterBeneficiaryResultDto>> Register(RegisterBeneficiaryRequest request, CancellationToken ct) =>
        Ok(await sender.Send(new RegisterBeneficiaryCommand(request.FirstName, request.FatherName,
            request.LastName, request.MotherName, request.BirthDate, request.IsBirthDateEstimated,
            request.Gender, request.EnrollmentDate, request.AreaId), ct));

    [HttpGet]
    [Route("GetAll")]
    [Authorize(Policy = Policies.ViewFacilityBeneficiaries)]
    public async Task<ActionResult<PagedResult<BeneficiaryListItemDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] Guid? facilityId = null, CancellationToken ct = default) =>
        Ok(await sender.Send(new GetBeneficiariesQuery(page, facilityId), ct));

    [HttpGet]
    [Route("PendingReview")]
    [Authorize(Policy = Policies.ReviewBeneficiaryMatches)]
    public async Task<ActionResult<IReadOnlyList<PendingBeneficiaryDto>>> PendingReview(CancellationToken ct) =>
        Ok(await sender.Send(new GetPendingBeneficiariesQuery(), ct));

    [HttpGet("{id:guid}/MatchCandidates")]
    [Authorize(Policy = Policies.ReviewBeneficiaryMatches)]
    public async Task<ActionResult<IReadOnlyList<BeneficiaryMatchDto>>> MatchCandidates(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetMatchCandidatesQuery(id), ct));

    [HttpPost("{id:guid}/ConfirmSame")]
    [Authorize(Policy = Policies.ReviewBeneficiaryMatches)]
    public async Task<IActionResult> ConfirmSame(Guid id, ConfirmBeneficiarySameRequest request, CancellationToken ct)
    {
        await sender.Send(new ConfirmBeneficiarySameCommand(id, request.ExistingBeneficiaryId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/ConfirmNotAMatch")]
    [Authorize(Policy = Policies.ReviewBeneficiaryMatches)]
    public async Task<IActionResult> ConfirmNotAMatch(Guid id, ConfirmBeneficiaryNotAMatchRequest request, CancellationToken ct)
    {
        await sender.Send(new ConfirmBeneficiaryNotAMatchCommand(id, request.Reason), ct);
        return NoContent();
    }
}
