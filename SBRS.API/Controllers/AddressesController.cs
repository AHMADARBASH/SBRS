using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.Addresses.Commands;
using SBRS.Application.Features.Addresses.DTOs;
using SBRS.Application.Features.Addresses.Queries;
using SBRS.Authorization;
using SBRS.Contracts.Addresses;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddressesController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet("GetByAreaId/{areaId:guid}")]
    public async Task<ActionResult<List<AddressDto>>> GetByArea(
        Guid areaId, [FromQuery] bool includeInactive, CancellationToken ct)
        => Ok(await sender.Send(new GetAddressesByAreaQuery(areaId, includeInactive), ct));

    [Authorize]
    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<AddressDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetAddressByIdQuery(id), ct));

    [Authorize(Policy = Policies.OrganizationLevel)]
    [HttpPost("Add")]
    public async Task<ActionResult<AddressDto>> Add(
        Guid areaId, AddAddressRequest body, CancellationToken ct)
    {
        var dto = await sender.Send(new AddAddressCommand(body.AreaId, body.ArName, body.EnName), ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [Authorize(Policy = Policies.OrganizationLevel)]
    [HttpPut("Update/{id:guid}")]
    public async Task<ActionResult<AddressDto>> Update(
        Guid id, UpdateAddressRequest body, CancellationToken ct)
        => Ok(await sender.Send(new UpdateAddressCommand(id, body.ArName, body.EnName), ct));

  
}