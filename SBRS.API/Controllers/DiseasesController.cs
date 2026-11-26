using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.Diseases.Commands;
using SBRS.Application.Features.Diseases.DTOs;
using SBRS.Application.Features.Diseases.Queries;
using SBRS.Authorization;
using SBRS.Contracts.Diseases;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiseasesController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet("GetAll")]
    public async Task<ActionResult<IReadOnlyList<DiseaseDto>>> GetAll(
        [FromQuery] Guid? diseaseCategoryId, CancellationToken ct) =>
        Ok(await sender.Send(new GetDiseasesQuery(diseaseCategoryId), ct));

    [Authorize]
    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<DiseaseDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetDiseaseByIdQuery(id), ct));

    [Authorize(Policy = Policies.ManageDiseases)]
    [HttpPost("Add")]
    public async Task<ActionResult<DiseaseDto>> Add(AddDiseaseRequest body, CancellationToken ct)
    {
        var dto = await sender.Send(new AddDiseaseCommand(
            body.ArName, body.EnName, body.Code, body.DiseaseCategoryId), ct);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [Authorize(Policy = Policies.ManageDiseases)]
    [HttpPut("Update")]
    public async Task<ActionResult<DiseaseDto>> Update(UpdateDiseaseRequest body, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateDiseaseCommand(
            body.Id, body.ArName, body.EnName, body.Code, body.DiseaseCategoryId), ct));
}
