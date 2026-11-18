using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class FacilityBeneficiaryRepository(AppDbContext db) : IFacilityBeneficiaryRepository
{
    public Task<bool> IsEnrolledAsync(Guid beneficiaryId, Guid facilityId, CancellationToken ct) =>
        db.FacilityBeneficiaries.AsNoTracking()
            .AnyAsync(x => x.BeneficiaryId == beneficiaryId && x.FacilityId == facilityId && x.IsActive, ct);

    // tracked on purpose: the enroll handler reactivates an inactive row
    public Task<FacilityBeneficiaryEntity?> GetAsync(Guid beneficiaryId, Guid facilityId, CancellationToken ct) =>
        db.FacilityBeneficiaries.FirstOrDefaultAsync(
            x => x.BeneficiaryId == beneficiaryId && x.FacilityId == facilityId, ct);

    // tracked on purpose: the merge moves these rows to the survivor
    public async Task<IReadOnlyList<FacilityBeneficiaryEntity>> GetByBeneficiaryIdAsync(
        Guid beneficiaryId, CancellationToken ct) =>
        await db.FacilityBeneficiaries
            .Where(x => x.BeneficiaryId == beneficiaryId)
            .ToListAsync(ct);

    public async Task<IReadOnlySet<Guid>> GetEnrolledBeneficiaryIdsAsync(
        IReadOnlyCollection<Guid> beneficiaryIds, Guid facilityId, CancellationToken ct)
    {
        if (beneficiaryIds.Count == 0) return new HashSet<Guid>();

        var ids = await db.FacilityBeneficiaries.AsNoTracking()
            .Where(x => x.FacilityId == facilityId && x.IsActive && beneficiaryIds.Contains(x.BeneficiaryId))
            .Select(x => x.BeneficiaryId)
            .Distinct()
            .ToListAsync(ct);

        return ids.ToHashSet();
    }

    public async Task AddAsync(FacilityBeneficiaryEntity enrollment, CancellationToken ct) =>
        await db.FacilityBeneficiaries.AddAsync(enrollment, ct);

    public void Remove(FacilityBeneficiaryEntity enrollment) =>
        db.FacilityBeneficiaries.Remove(enrollment);
}
