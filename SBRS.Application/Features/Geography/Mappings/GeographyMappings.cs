using SBRS.Application.Features.Geography.DTOs;
namespace SBRS.Application.Features.Geography.Mappings
{
    public static class GeographyMappings
    {
        public static CountryDto ToDto(this CountryEntity e) => new(e.Id, e.ArName, e.EnName, e.Code);
        public static CityDto ToDto(this CityEntity e) => new(e.Id, e.CountryId, e.ArName, e.EnName, e.Code);
        public static RegionDto ToDto(this RegionEntity e) => new(e.Id, e.CityId, e.ArName, e.EnName, e.Code);
        public static DistrictDto ToDto(this DistrictEntity e) => new(e.Id, e.RegionId, e.ArName, e.EnName, e.Code);
        public static AreaDto ToDto(this AreaEntity e) => new(e.Id, e.DistrictId, e.ArName, e.EnName, e.Code);
    }
}
