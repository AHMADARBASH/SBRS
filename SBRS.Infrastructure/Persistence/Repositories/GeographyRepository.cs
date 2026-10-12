using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories
{
    public class GeographyRepository(AppDbContext db) : IGeographyRepository
    {
        public async Task<List<CountryEntity>> GetCountriesAsync(CancellationToken ct = default)
            => await db.Countries.AsNoTracking().OrderBy(c => c.EnName).ToListAsync(ct);

        public async Task<List<CityEntity>> GetCitiesByCountryAsync(Guid countryId, CancellationToken ct = default)
            => await db.Cities.AsNoTracking().Where(c => c.CountryId == countryId).OrderBy(c => c.EnName).ToListAsync(ct);
        

        public async Task<List<RegionEntity>> GetRegionsByCityAsync(Guid cityId, CancellationToken ct = default)
            => await db.Regions.AsNoTracking().Where(r => r.CityId == cityId).OrderBy(r => r.EnName).ToListAsync(ct);

        public async Task<List<DistrictEntity>> GetDistrictsByRegionAsync(Guid regionId, CancellationToken ct = default)
            => await db.Districts.AsNoTracking().Where(d => d.RegionId == regionId).OrderBy(d => d.EnName).ToListAsync(ct);

        public async Task<List<AreaEntity>> GetAreasByDistrictAsync(Guid districtId, CancellationToken ct = default)
            => await db.Areas.AsNoTracking().Where(a => a.DistrictId == districtId).OrderBy(a => a.EnName).ToListAsync(ct);
    }
}
