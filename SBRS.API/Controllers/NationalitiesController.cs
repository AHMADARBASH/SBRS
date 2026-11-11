using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.Nationalities.Commands;
using SBRS.Application.Features.Nationalities.DTOs;
using SBRS.Application.Features.Nationalities.Queries;
using SBRS.Authorization;
using SBRS.Contracts.Nationalities;

namespace SBRS.Controllers;

[ApiController]
[Route("api/nationalities")]
public class NationalitiesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("GetAll")]
    public async Task<ActionResult<IReadOnlyList<NationalityDto>>> GetAll(CancellationToken ct = default) =>
        Ok(await sender.Send(new GetNationalitiesQuery(), ct));

    [HttpPost]
    [Route("Add")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<NationalityDto>> Add(AddNationalityRequest r, CancellationToken ct)
    {
        var nationality = await sender.Send(new AddNationalityCommand(r.ArName, r.EnName), ct);
        return StatusCode(StatusCodes.Status201Created, nationality);
    }

    [HttpPut()]
    [Route("Update")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<NationalityDto>> Update(UpdateNationalityRequest r, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateNationalityCommand(r.Id, r.ArName, r.EnName), ct));
}
