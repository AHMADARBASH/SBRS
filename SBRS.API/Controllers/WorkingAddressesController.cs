using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Models;
using SBRS.Application.Features.WorkingAddresses.Commands;
using SBRS.Application.Features.WorkingAddresses.DTOs;
using SBRS.Application.Features.WorkingAddresses.Queries;
using SBRS.Authorization;
using SBRS.Contracts.WorkingAddresses;
using SBRS.Domain.Exceptions;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkingAddressesController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    private Guid FacilityId => currentUser.FacilityId
        ?? throw new DomainException("No facility is assigned to this user.");

    [Authorize(Policy = Policies.RecordWorkingAddresses)]
    [HttpPost("Add")]
    public async Task<ActionResult<WorkingAddressDto>> Add(AddWorkingAddressRequest body, CancellationToken ct)
    {
        var dto = await sender.Send(new AddWorkingAddressCommand(
            FacilityId, body.WorkingDate, body.TypeId, body.AddressId,
            body.NewAddress is null
                ? null
                : new NewWorkingAddress(body.NewAddress.AreaId, body.NewAddress.ArName,
                    body.NewAddress.EnName)), ct);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [Authorize(Policy = Policies.UpdateWorkingAddresses)]
    [HttpPut("Update")]
    public async Task<ActionResult<WorkingAddressDto>> Update(UpdateWorkingAddressRequest body,
        CancellationToken ct) =>
        Ok(await sender.Send(new UpdateWorkingAddressCommand(
            body.Id, FacilityId, body.WorkingDate, body.TypeId), ct));

    [Authorize(Policy = Policies.DeleteWorkingAddresses)]
    [HttpDelete("Delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteWorkingAddressCommand(id, FacilityId), ct);
        return NoContent();
    }

    [Authorize(Policy = Policies.ViewWorkingAddresses)]
    [HttpGet("GetAll")]
    public async Task<ActionResult<PagedResult<WorkingAddressDto>>> GetAll(
        [FromQuery] int page = 1, CancellationToken ct = default) =>
        Ok(await sender.Send(new GetWorkingAddressesQuery(FacilityId, page), ct));

    [Authorize(Policy = Policies.ViewWorkingAddresses)]
    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<WorkingAddressDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetWorkingAddressByIdQuery(id, FacilityId), ct));
}
