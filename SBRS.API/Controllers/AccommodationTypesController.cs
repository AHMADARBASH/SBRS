using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.AccommodationTypes.Commands;
using SBRS.Application.Features.AccommodationTypes.DTOs;
using SBRS.Application.Features.AccommodationTypes.Queries;
using SBRS.Authorization;
using SBRS.Contracts.AccommodationTypes;

namespace SBRS.Controllers;

[ApiController]
[Route("api/accommodation-types")]
public class AccommodationTypesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("GetAll")]
    public async Task<ActionResult<IReadOnlyList<AccommodationTypeDto>>> GetAll(CancellationToken ct = default) =>
        Ok(await sender.Send(new GetAccommodationTypesQuery(), ct));

    [HttpPost]
    [Route("Add")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<AccommodationTypeDto>> Add(AddAccommodationTypeRequest r, CancellationToken ct)
    {
        var accommodationType = await sender.Send(new AddAccommodationTypeCommand(r.ArName, r.EnName), ct);
        return StatusCode(StatusCodes.Status201Created, accommodationType);
    }

    [HttpPut()]
    [Route("Update")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<AccommodationTypeDto>> Update(UpdateAccommodationTypeRequest r, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateAccommodationTypeCommand(r.Id, r.ArName, r.EnName), ct));
}
