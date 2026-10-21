using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.IdentityTypes.Commands;
using SBRS.Application.Features.IdentityTypes.DTOs;
using SBRS.Application.Features.IdentityTypes.Queries;
using SBRS.Authorization;
using SBRS.Contracts.IdentityTypes;

namespace SBRS.Controllers;

[ApiController]
[Route("api/identity-types")]
public class IdentityTypesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("GetAll")]
    public async Task<ActionResult<IReadOnlyList<IdentityTypeDto>>> GetAll(CancellationToken ct = default) =>
        Ok(await sender.Send(new GetIdentityTypesQuery(), ct));

    [HttpPost]
    [Route("Add")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<IdentityTypeDto>> Add(AddIdentityTypeRequest r, CancellationToken ct)
    {
        var identityType = await sender.Send(new AddIdentityTypeCommand(r.ArName, r.EnName), ct);
        return StatusCode(StatusCodes.Status201Created, identityType);
    }

    [HttpPut()]
    [Route("Update")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<IdentityTypeDto>> Update(UpdateIdentityTypeRequest r, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateIdentityTypeCommand(r.Id, r.ArName, r.EnName), ct));
}
