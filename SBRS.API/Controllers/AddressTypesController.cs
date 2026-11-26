using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.AddressTypes.Commands;
using SBRS.Application.Features.AddressTypes.DTOs;
using SBRS.Application.Features.AddressTypes.Queries;
using SBRS.Authorization;
using SBRS.Contracts.AddressTypes;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddressTypesController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet("GetAll")]
    public async Task<ActionResult<IReadOnlyList<AddressTypeDto>>> GetAll(CancellationToken ct) =>
        Ok(await sender.Send(new GetAddressTypesQuery(), ct));

    [Authorize]
    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<AddressTypeDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetAddressTypeByIdQuery(id), ct));

    [Authorize(Policy = Policies.ManageAddressTypes)]
    [HttpPost("Add")]
    public async Task<ActionResult<AddressTypeDto>> Add(AddAddressTypeRequest body, CancellationToken ct)
    {
        var dto = await sender.Send(new AddAddressTypeCommand(body.ArName, body.EnName), ct);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [Authorize(Policy = Policies.ManageAddressTypes)]
    [HttpPut("Update")]
    public async Task<ActionResult<AddressTypeDto>> Update(UpdateAddressTypeRequest body, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateAddressTypeCommand(body.Id, body.ArName, body.EnName), ct));
}
