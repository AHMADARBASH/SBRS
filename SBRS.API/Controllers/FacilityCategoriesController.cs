using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.FacilityCategories.Commands;
using SBRS.Application.Features.FacilityCategories.DTOs;
using SBRS.Application.Features.FacilityCategories.Queries;
using SBRS.Authorization;
using SBRS.Contracts.FacilityCategories;

namespace SBRS.Controllers;

[ApiController]
[Route("api/FacilityCategories")]
public class FacilityCategoriesController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet("GetAll")]
    public async Task<ActionResult<IReadOnlyList<FacilityCategoryDto>>> GetAll(CancellationToken ct = default) =>
        Ok(await sender.Send(new GetFacilityCategoriesQuery(), ct));

    [Authorize(Policy = Policies.SystemAdminOnly)]
    [HttpPost("Add")]
    public async Task<ActionResult<FacilityCategoryDto>> Add(
        AddFacilityCategoryRequest r, CancellationToken ct)
    {
        var category = await sender.Send(new AddFacilityCategoryCommand(r.ArName, r.EnName, r.Code), ct);
        return StatusCode(StatusCodes.Status201Created, category);
    }

    [Authorize]
    [HttpGet("GetById/{id:guid}")]
    public async Task<ActionResult<FacilityCategoryDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetFacilityCategoryByIdQuery(id), ct));

    [Authorize(Policy = Policies.SystemAdminOnly)]
    [HttpPut("Update/{id:guid}")]
    public async Task<ActionResult<FacilityCategoryDto>> Update(
        Guid id, UpdateFacilityCategoryRequest r, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateFacilityCategoryCommand(id, r.ArName, r.EnName, r.Code), ct));
}
