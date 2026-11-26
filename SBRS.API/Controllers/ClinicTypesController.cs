using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.ClinicTypes.Commands;
using SBRS.Application.Features.ClinicTypes.DTOs;
using SBRS.Application.Features.ClinicTypes.Queries;
using SBRS.Authorization;
using SBRS.Contracts.ClinicTypes;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicTypesController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet("GetAll")]
    public async Task<ActionResult<IReadOnlyList<ClinicTypeDto>>> GetAll(CancellationToken ct) =>
        Ok(await sender.Send(new GetClinicTypesQuery(), ct));

    [Authorize(Policy = Policies.ManageClinicTypes)]
    [HttpPost("Add")]
    public async Task<ActionResult<ClinicTypeDto>> Add(AddClinicTypeRequest body, CancellationToken ct)
    {
        var dto = await sender.Send(new AddClinicTypeCommand(body.ArName, body.EnName, body.Code), ct);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [Authorize(Policy = Policies.ManageClinicTypes)]
    [HttpPut("Update")]
    public async Task<ActionResult<ClinicTypeDto>> Update(UpdateClinicTypeRequest body, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateClinicTypeCommand(body.Id, body.ArName, body.EnName, body.Code), ct));
}
