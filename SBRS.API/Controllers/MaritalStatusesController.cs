using MediatR;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.MaritalStatuses.Queries;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaritalStatusesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("GetAll")]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await sender.Send(new GetMaritalStatusesQuery(), ct));
}
