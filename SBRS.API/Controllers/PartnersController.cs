using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.Partners.Commands;
using SBRS.Application.Features.Partners.DTOs;
using SBRS.Application.Features.Partners.Queries;
using SBRS.Authorization;
using SBRS.Contracts.Partners;

namespace SBRS.Controllers;

[ApiController]
[Route("api/partners")]
public class PartnersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("GetAll")]
    public async Task<ActionResult<IReadOnlyList<PartnerDto>>> GetAll(CancellationToken ct) =>
        Ok(await sender.Send(new GetPartnersQuery(), ct));

    [HttpPost]
    [Route("Add")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<PartnerDto>> Add(AddPartnerRequest r, CancellationToken ct)
    {
        var partner = await sender.Send(new AddPartnerCommand(r.ArName, r.EnName, r.Code), ct);
        return StatusCode(StatusCodes.Status201Created, partner);
    }

    [HttpPut()]
    [Route("Update")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<PartnerDto>> Update(UpdatePartnerRequest r, CancellationToken ct) =>
        Ok(await sender.Send(new UpdatePartnerCommand(r.Id, r.ArName, r.EnName, r.Code), ct));
}