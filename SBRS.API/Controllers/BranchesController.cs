using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.Branches.Commands;
using SBRS.Application.Features.Branches.Queries;
using SBRS.Application.Features.Branches.Queries.GetBranchesByOrganization;
using SBRS.Authorization;
using SBRS.Contracts.Branches;
using SBRS.Contracts.Organizations;

namespace SBRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController(ISender sender) : ControllerBase
    {
        [Authorize]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive, CancellationToken ct)
        => Ok(await sender.Send(new GetBranchesQuery(), ct));


        [Authorize]
        [HttpGet("GetById/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetBranchByIdQuery(id), ct));


        [Authorize]
        [HttpGet("GetByOrganizationId/{organizationId:guid}")]
        public async Task<IActionResult> GetByOrganizationId([FromRoute] Guid organizationId, CancellationToken ct)
        => Ok(await sender.Send(new GetBranchesByOrganizationIdQuery(organizationId), ct));

        [Authorize(Policy = Policies.OrganizationLevel)]
        [HttpPost("Add")]
        public async Task<IActionResult> Add(AddBranchRequest body, CancellationToken ct)
        {
            var dto = await sender.Send(new AddBranchCommand(body.OrganizationId,body.ArName, body.EnName), ct);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        [Authorize(Policy = Policies.OrganizationLevel)]
        [HttpPut("Update/{id:guid}")]
        public async Task<IActionResult> Update(
        Guid id, UpdateBranchRequest body, CancellationToken ct)
        => Ok(await sender.Send(new UpdateBranchCommand(id, body.ArName, body.EnName), ct));


    }
}
