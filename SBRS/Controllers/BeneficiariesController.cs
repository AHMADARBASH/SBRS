using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Common.Models;
using SBRS.Application.Features.Beneficiaries.Commands;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Queries;
using SBRS.Application.Features.BeneficiaryProfiles.Commands;
using SBRS.Application.Features.BeneficiaryProfiles.DTOs;
using SBRS.Application.Features.BeneficiaryProfiles.Queries;
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
            request.Gender, request.EnrollmentDate, request.AreaId, request.NationalityId,
            request.IdentityTypeId, request.IdentityValue), ct));

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

    [HttpGet("{id:guid}/Identities")]
    [Authorize(Policy = Policies.ViewFacilityBeneficiaries)]
    public async Task<ActionResult<IReadOnlyList<BeneficiaryIdentityDto>>> Identities(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetBeneficiaryIdentitiesQuery(id), ct));

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

    // Deletes the record visually for the frontend, no hard delete: the server only sets IsActive = false.
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.DeactivateBeneficiaries)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeactivateBeneficiaryCommand(id), ct);
        return NoContent();
    }

    // The sensitive profile step: 201 on create, 200 on update (full replace every time).
    [HttpPut]
    [Route("SaveProfile/{beneficiaryId:guid}")]
    [Authorize(Policy = Policies.ManageBeneficiaryProfiles)]
    public async Task<ActionResult<BeneficiaryProfileDto>> SaveProfile(
        Guid beneficiaryId, [FromBody] SaveBeneficiaryProfileRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new SaveBeneficiaryProfileCommand(
            beneficiaryId, request.PhoneNumber1, request.PhoneNumber2, request.IsDisplaced,
            request.DisplacementDate, request.OldAreaId, request.AccommodationTypeId,
            request.IsDisabled,
            (request.Disabilities ?? [])
                .Select(d => new SaveBeneficiaryDisabilityItem(d.DisabilityTypeId, d.ReasonId)).ToList(),
            request.ChronicDiseaseIds, request.BeneficiaryStatusId,
            request.CurrentWork, request.PreviousWork,
            request.EducationTypeId, request.EducationIssueId,
            request.MaritalStatusId, request.WivesNumber, request.ChildrenNumber), ct);

        return result.Created
            ? CreatedAtAction(nameof(GetProfile), new { beneficiaryId }, result.Profile)
            : Ok(result.Profile);
    }

    // Same endpoint family: reads also answer 404 until a profile was saved.
    [HttpGet]
    [Route("GetProfile/{beneficiaryId:guid}")]
    [Authorize(Policy = Policies.ViewFacilityBeneficiaries)]
    public async Task<ActionResult<BeneficiaryProfileDto>> GetProfile(Guid beneficiaryId, CancellationToken ct) =>
        Ok(await sender.Send(new GetBeneficiaryProfileQuery(beneficiaryId), ct));
}
