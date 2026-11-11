using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.FacilityPartners.Commands;
using SBRS.Application.Features.FacilityPartners.DTOs;
using SBRS.Application.Features.FacilityPartners.Queries;
using SBRS.Authorization;
using SBRS.Contracts.FacilityPartners;

namespace SBRS.Controllers;

[ApiController]
[Route("api/facility-partners")]
[Authorize(Policy = Policies.SystemAdminOnly)]
public class FacilityPartnersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("GetByFacility/{facilityId:guid}")]
    public async Task<ActionResult<IReadOnlyList<FacilityPartnerDto>>> GetByFacility(Guid facilityId, CancellationToken ct) =>
        Ok(await sender.Send(new GetFacilityPartnersQuery(facilityId), ct));

    [HttpPost]
    [Route("Assign")]
    public async Task<ActionResult<FacilityPartnerDto>> Assign(AssignFacilityPartnerRequest r, CancellationToken ct)
    {
        var entry = await sender.Send(
            new AssignFacilityPartnerCommand(r.FacilityId, r.PartnerId, r.ProjectCode, r.StartDate), ct);
        return StatusCode(StatusCodes.Status201Created, entry);
    }
}