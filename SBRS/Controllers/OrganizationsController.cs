using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Commands;
using SBRS.Domain.Entities;

namespace SBRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController(ISender sender) : ControllerBase
    {
        [HttpPost("AddOrganization")]
        public async Task<IActionResult> AddOrganization([FromBody] OrganizationEntity organization)
        {
            var result = await sender.Send(new AddOrganizationCommand(organization));
            return Ok(result);
        }
    }
}
