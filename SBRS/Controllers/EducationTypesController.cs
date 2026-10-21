using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.EducationTypes.Commands;
using SBRS.Application.Features.EducationTypes.DTOs;
using SBRS.Application.Features.EducationTypes.Queries;
using SBRS.Authorization;
using SBRS.Contracts.EducationTypes;

namespace SBRS.Controllers;

[ApiController]
[Route("api/education-types")]
public class EducationTypesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("GetAll")]
    public async Task<ActionResult<IReadOnlyList<EducationTypeDto>>> GetAll(CancellationToken ct = default) =>
        Ok(await sender.Send(new GetEducationTypesQuery(), ct));

    [HttpPost]
    [Route("Add")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<EducationTypeDto>> Add(AddEducationTypeRequest r, CancellationToken ct)
    {
        var educationType = await sender.Send(new AddEducationTypeCommand(r.ArName, r.EnName), ct);
        return StatusCode(StatusCodes.Status201Created, educationType);
    }

    [HttpPut()]
    [Route("Update")]
    [Authorize(Policy = Policies.SystemAdminOnly)]
    public async Task<ActionResult<EducationTypeDto>> Update(UpdateEducationTypeRequest r, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateEducationTypeCommand(r.Id, r.ArName, r.EnName), ct));
}
