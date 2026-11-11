using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.DisabilityTypes.Commands;
using SBRS.Application.Features.DisabilityTypes.DTOs;
using SBRS.Application.Features.DisabilityTypes.Queries;
using SBRS.Authorization;
using SBRS.Contracts.DisabilityTypes;

namespace SBRS.Controllers;

[ApiController]
[Route("api/disability-types")]
public class DisabilityTypesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("GetAll")]
    public async Task<ActionResult<IReadOnlyList<DisabilityTypeDto>>> GetAll(CancellationToken ct = default) =>
        Ok(await sender.Send(new GetDisabilityTypesQuery(), ct));

    [HttpPost]
    [Route("Add")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<DisabilityTypeDto>> Add(AddDisabilityTypeRequest r, CancellationToken ct)
    {
        var disabilityType = await sender.Send(new AddDisabilityTypeCommand(r.ArName, r.EnName), ct);
        return StatusCode(StatusCodes.Status201Created, disabilityType);
    }

    [HttpPut()]
    [Route("Update")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<DisabilityTypeDto>> Update(UpdateDisabilityTypeRequest r, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateDisabilityTypeCommand(r.Id, r.ArName, r.EnName), ct));
}
