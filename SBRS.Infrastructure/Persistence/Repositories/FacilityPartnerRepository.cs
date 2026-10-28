using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class FacilityPartnerRepository(AppDbContext db) : IFacilityPartnerRepository
{
    public Task<FacilityPartnerEntity?> GetCurrentAsync(Guid facilityId, CancellationToken ct) =>
        db.FacilitiesPartners.FirstOrDefaultAsync(x => x.FacilityId == facilityId && x.EndDate == null, ct);

    public async Task<IReadOnlyList<FacilityPartnerEntity>> GetByFacilityAsync(Guid facilityId, CancellationToken ct) =>
        await db.FacilitiesPartners.AsNoTracking()
            .Include(x => x.Partner)
            .Where(x => x.FacilityId == facilityId)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(ct);

    public Task AddAsync(FacilityPartnerEntity entry, CancellationToken ct)
    {
        db.FacilitiesPartners.Add(entry);
        return Task.CompletedTask;
    }

    public void RemoveStaged(FacilityPartnerEntity entry)
        => db.Entry(entry).State = EntityState.Detached;
}