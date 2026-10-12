using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Features.Geography.DTOs;
using SBRS.Application.Features.Geography.Queries;

namespace SBRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeographyController(ISender sender) : ControllerBase
    {
        [HttpGet("Countries")]
        public async Task<IActionResult> GetCountries()
             => Ok(await sender.Send(new GetCountriesQuery()));
        

        [HttpGet("countries/{countryId:guid}/cities")]
        public async Task<IActionResult> GetCities(
        Guid countryId, [FromQuery] bool includeInactive, CancellationToken ct)
            => Ok(await sender.Send(new GetCitiesByCountryQuery(countryId), ct));


        [HttpGet("cities/{cityId:guid}/regions")]
        public async Task<ActionResult<List<RegionDto>>> GetRegions(
       Guid cityId, [FromQuery] bool includeInactive, CancellationToken ct)
       => Ok(await sender.Send(new GetRegionsByCityQuery(cityId), ct));

        [HttpGet("regions/{regionId:guid}/districts")]
        public async Task<ActionResult<List<DistrictDto>>> GetDistricts(
            Guid regionId, [FromQuery] bool includeInactive, CancellationToken ct)
            => Ok(await sender.Send(new GetDistrictsByRegionQuery(regionId), ct));

        [HttpGet("districts/{districtId:guid}/areas")]
        public async Task<ActionResult<List<AreaDto>>> GetAreas(
            Guid districtId, [FromQuery] bool includeInactive, CancellationToken ct)
            => Ok(await sender.Send(new GetAreasByDistrictQuery(districtId), ct));


    }
}
