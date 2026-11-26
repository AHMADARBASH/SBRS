using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.DiseaseCategories.Commands;
using SBRS.Application.Features.DiseaseCategories.DTOs;
using SBRS.Application.Features.DiseaseCategories.Queries;
using SBRS.Authorization;
using SBRS.Contracts.DiseaseCategories;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiseaseCategoriesController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet("GetAll")]
    public async Task<ActionResult<IReadOnlyList<DiseaseCategoryDto>>> GetAll(CancellationToken ct) =>
        Ok(await sender.Send(new GetDiseaseCategoriesQuery(), ct));

    [Authorize]
    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<DiseaseCategoryDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetDiseaseCategoryByIdQuery(id), ct));

    [Authorize(Policy = Policies.ManageDiseaseCategories)]
    [HttpPost("Add")]
    public async Task<ActionResult<DiseaseCategoryDto>> Add(AddDiseaseCategoryRequest body, CancellationToken ct)
    {
        var dto = await sender.Send(new AddDiseaseCategoryCommand(
            body.ArName, body.EnName, body.Code), ct);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [Authorize(Policy = Policies.ManageDiseaseCategories)]
    [HttpPut("Update")]
    public async Task<ActionResult<DiseaseCategoryDto>> Update(UpdateDiseaseCategoryRequest body,
        CancellationToken ct) =>
        Ok(await sender.Send(new UpdateDiseaseCategoryCommand(
            body.Id, body.ArName, body.EnName, body.Code), ct));
}
