using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class WorkingAddressRepository(AppDbContext db) : IWorkingAddressRepository
{
    // One query walks the whole chain with joins; no per-row lookups.
    private IQueryable<WorkingAddressRow> Rows(Guid facilityId) =>
        from wa in db.WorkingAddresses.AsNoTracking()
        join type in db.AddressTypes on wa.TypeId equals type.Id
        join address in db.Addresses on wa.AddressId equals address.Id
        join area in db.Areas on address.AreaId equals area.Id
        join district in db.Districts on area.DistrictId equals district.Id
        join region in db.Regions on district.RegionId equals region.Id
        join city in db.Cities on area.CityId equals city.Id
        join country in db.Countries on city.CountryId equals country.Id
        where wa.FacilityId == facilityId && wa.IsActive
        select new WorkingAddressRow(
            wa.Id, wa.FacilityId, wa.WorkingDate, wa.IsActive, wa.CreatedAt,
            type.Id, type.ArName, type.EnName,
            address.Id, address.ArName, address.EnName,
            area.Id, area.ArName, area.EnName,
            district.Id, district.ArName, district.EnName,
            region.Id, region.ArName, region.EnName,
            city.Id, city.ArName, city.EnName,
            country.Id, country.ArName, country.EnName);

    public Task<WorkingAddressEntity?> GetByIdTrackedAsync(Guid id, Guid facilityId, CancellationToken ct) =>
        db.WorkingAddresses.FirstOrDefaultAsync(
            w => w.Id == id && w.FacilityId == facilityId && w.IsActive, ct);

    public Task<WorkingAddressRow?> GetByIdAsync(Guid id, Guid facilityId, CancellationToken ct) =>
        Rows(facilityId).FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<(IReadOnlyList<WorkingAddressRow> Items, int TotalCount)> GetPagedAsync(
        Guid facilityId, int page, int pageSize, CancellationToken ct)
    {
        var query = Rows(facilityId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.WorkingDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<bool> DuplicateActiveAsync(Guid facilityId, Guid addressId, DateOnly workingDate,
        Guid? excludeId, CancellationToken ct) =>
        db.WorkingAddresses.AnyAsync(w =>
            w.IsActive && w.FacilityId == facilityId && w.AddressId == addressId &&
            w.WorkingDate == workingDate && (excludeId == null || w.Id != excludeId), ct);

    public async Task AddAsync(WorkingAddressEntity workingAddress, CancellationToken ct) =>
        await db.WorkingAddresses.AddAsync(workingAddress, ct);
}
