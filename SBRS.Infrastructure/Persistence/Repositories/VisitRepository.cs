using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class VisitRepository(AppDbContext db) : IVisitRepository
{
    public Task<VisitEntity?> GetByIdAsync(Guid id, Guid facilityId, CancellationToken ct) =>
        db.Visits
            .Include(v => v.Beneficiary)
            .FirstOrDefaultAsync(v => v.Id == id && v.FacilityId == facilityId && v.IsActive, ct);

    public async Task<(IReadOnlyList<VisitEntity> Items, int TotalCount)> GetPagedAsync(Guid facilityId,
        Guid? beneficiaryId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Visits.AsNoTracking()
            .Include(v => v.Beneficiary)
            .Where(v => v.FacilityId == facilityId && v.IsActive)
            .Where(v => beneficiaryId == null || v.BeneficiaryId == beneficiaryId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(v => v.VisitDate)
            .ThenByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<bool> ExistsActiveOnDateAsync(Guid facilityId, Guid beneficiaryId, DateOnly visitDate,
        Guid? excludeId, CancellationToken ct) =>
        db.Visits.AnyAsync(v => v.FacilityId == facilityId && v.BeneficiaryId == beneficiaryId &&
                                v.VisitDate == visitDate && v.IsActive &&
                                (excludeId == null || v.Id != excludeId), ct);

    public async Task AddAsync(VisitEntity visit, CancellationToken ct) =>
        await db.Visits.AddAsync(visit, ct);
}
