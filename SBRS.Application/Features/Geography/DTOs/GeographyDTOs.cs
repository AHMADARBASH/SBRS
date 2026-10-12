using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Features.Geography.DTOs
{
    public record CountryDto(Guid Id, string ArName, string EnName, string Code);
    public record CityDto(Guid Id, Guid CountryId, string ArName, string EnName, string Code);
    public record RegionDto(Guid Id, Guid CityId, string ArName, string EnName, string Code);
    public record DistrictDto(Guid Id, Guid RegionId, string ArName, string EnName, string Code);
    public record AreaDto(Guid Id, Guid DistrictId, string ArName, string EnName, string Code);
}
