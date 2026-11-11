using MediatR;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.ChronicDiseases.Queries;

namespace SBRS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChronicDiseasesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("GetAll")]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await sender.Send(new GetChronicDiseasesQuery(), ct));
}
