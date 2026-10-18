namespace SBRS.Domain.Interfaces
{
    public interface IGeographyRepository
    {
        Task<List<CountryEntity>> GetCountriesAsync( CancellationToken ct = default);
        Task<List<CityEntity>> GetCitiesByCountryAsync(Guid countryId, CancellationToken ct = default);
        Task<List<RegionEntity>> GetRegionsByCityAsync(Guid cityId, CancellationToken ct = default);
        Task<List<DistrictEntity>> GetDistrictsByRegionAsync(Guid regionId, CancellationToken ct = default);
        Task<List<AreaEntity>> GetAreasByDistrictAsync(Guid districtId, CancellationToken ct = default);
        Task<AreaEntity?> GetAreaByIdAsync(Guid id, CancellationToken ct = default);
    }
}
