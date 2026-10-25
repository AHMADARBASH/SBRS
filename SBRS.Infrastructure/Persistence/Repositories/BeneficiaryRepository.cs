using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class BeneficiaryRepository(AppDbContext db) : IBeneficiaryRepository
{
    public Task<BeneficiaryEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Beneficiaries.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<BeneficiaryEntity>> GetByMatchKeyAsync(string matchKey, CancellationToken ct) =>
        await db.Beneficiaries.AsNoTracking()
            .Where(x => x.MatchKey == matchKey)
            .OrderBy(x => x.Disambiguator)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BeneficiaryEntity>> GetProbableCandidatesAsync(BeneficiaryIdentityKey key,
        CancellationToken ct)
    {
        var from = new DateOnly(key.BirthDate.Year, 1, 1);
        var to = new DateOnly(key.BirthDate.Year, 12, 31);

        // Inactive beneficiaries are included on purpose: a deactivated person must not be registered again by mistake.
        return await db.Beneficiaries.AsNoTracking()
            .Where(x => x.BirthDate >= from && x.BirthDate <= to &&
                        (x.NormalizedLastName == key.LastName ||
                         x.NormalizedMotherName == key.MotherName ||
                         x.NormalizedFirstName == key.FirstName))
            .Take(200)
            .ToListAsync(ct);
    }

    public async Task<int> GetNextDisambiguatorAsync(string matchKey, CancellationToken ct)
    {
        var max = await db.Beneficiaries
            .Where(x => x.MatchKey == matchKey)
            .Select(x => (int?)x.Disambiguator)
            .MaxAsync(ct);

        return max.HasValue ? max.Value + 1 : 0;
    }

    public async Task AddAsync(BeneficiaryEntity beneficiary, CancellationToken ct) =>
        await db.Beneficiaries.AddAsync(beneficiary, ct);

    public async Task<int> GetNextCodeAsync(CancellationToken ct) =>
        await db.Database
            .SqlQuery<int>($"SELECT NEXT VALUE FOR dbo.BeneficiaryCodeSeq AS [Value]")
            .SingleAsync(ct);

    public async Task<IReadOnlyList<BeneficiaryEntity>> GetPendingReviewAsync(Guid? facilityId, CancellationToken ct) =>
        await db.Beneficiaries.AsNoTracking()
            .Where(x => x.MatchReviewStatus == BeneficiaryMatchReviewStatus.PendingReview && x.IsActive &&
                        (facilityId == null || x.RegistrationFacilityId == facilityId))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
}
