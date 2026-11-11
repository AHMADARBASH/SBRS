using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.FacilityTypes.Commands;
using SBRS.Application.Features.FacilityTypes.DTOs;
using SBRS.Application.Features.FacilityTypes.Queries;
using SBRS.Authorization;
using SBRS.Contracts.FacilityTypes;

namespace SBRS.Controllers;

[ApiController]
[Route("api/facility-types")]
public class FacilityTypesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("GetAll")]
    public async Task<ActionResult<IReadOnlyList<FacilityTypeDto>>> GetAll(
        [FromQuery] bool includeInactive = false, CancellationToken ct = default) =>
        Ok(await sender.Send(new GetFacilityTypesQuery(includeInactive), ct));

    [HttpPost]
    [Route("Add")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<FacilityTypeDto>> Add(AddFacilityTypeRequest r, CancellationToken ct)
    {
        var type = await sender.Send(new AddFacilityTypeCommand(r.ArName, r.EnName, r.Code), ct);
        return StatusCode(StatusCodes.Status201Created, type);
    }

    [HttpPut()]
    [Route("Update")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<FacilityTypeDto>> Update( UpdateFacilityTypeRequest r, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateFacilityTypeCommand(r.Id, r.ArName, r.EnName, r.Code), ct));

   
}