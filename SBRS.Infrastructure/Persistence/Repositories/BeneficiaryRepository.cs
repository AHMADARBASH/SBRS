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

    public async Task<IReadOnlyList<BeneficiaryEntity>> GetByExactIdentityAsync(
        IReadOnlyCollection<(Guid IdentityTypeId, string Value)> identities,
        Guid? excludeBeneficiaryId, CancellationToken ct)
    {
        // "" is the UNREGISTERED value: every beneficiary may hold it, so it never 100%-matches.
        var pairs = identities.Where(i => i.Value != string.Empty).ToList();
        if (pairs.Count == 0) return Array.Empty<BeneficiaryEntity>();

        var typeIds = pairs.Select(p => p.IdentityTypeId).Distinct().ToList();

        var query = db.BeneficiaryIdentities.AsNoTracking()
            .Where(x => x.Value != string.Empty && typeIds.Contains(x.IdentityTypeId));

        if (excludeBeneficiaryId is { } excluded)
            query = query.Where(x => x.BeneficiaryId != excluded);

        var rows = await query
            .Select(x => new { x.BeneficiaryId, x.IdentityTypeId, x.Value })
            .ToListAsync(ct);

        // keep only the rows whose (type, value) pair is one of the wanted pairs
        var wanted = pairs.ToHashSet();
        var resultIds = rows
            .Where(r => wanted.Contains((r.IdentityTypeId, r.Value)))
            .Select(r => r.BeneficiaryId)
            .Distinct()
            .ToList();

        if (resultIds.Count == 0) return Array.Empty<BeneficiaryEntity>();

        return await db.Beneficiaries.AsNoTracking()
            .Where(b => resultIds.Contains(b.Id))
            .ToListAsync(ct);
    }

    public async Task AddAsync(BeneficiaryEntity beneficiary, CancellationToken ct) =>
        await db.Beneficiaries.AddAsync(beneficiary, ct);

    /// <summary>
    /// Highest Code used at the given facility, or 0 when none exists yet.
    /// Includes deactivated rows: beneficiaries are never hard-deleted (deactivate instead),
    /// so the max Code is permanent and no number is ever reused; a hard delete would allow
    /// code reuse, which must not happen because codes may already be printed or shared.
    /// Call within the facility row lock (LockRegistrationFacilityAsync) to be race-safe.
    /// </summary>
    public async Task<int> GetMaxCodeForFacilityAsync(Guid facilityId, CancellationToken ct)
    {
        var max = await db.Beneficiaries.IgnoreQueryFilters()
            .Where(x => x.RegistrationFacilityId == facilityId)
            .Select(x => (int?)x.Code)
            .MaxAsync(ct);

        return max.HasValue ? max.Value : 0;
    }

    public Task LockRegistrationFacilityAsync(Guid facilityId, CancellationToken ct) =>
        db.Database.ExecuteSqlAsync(
            $"SELECT 1 FROM facilities WHERE id = {facilityId} FOR UPDATE", ct);

    public async Task<IReadOnlyList<BeneficiaryEntity>> GetPendingReviewAsync(Guid? facilityId, CancellationToken ct) =>
        await db.Beneficiaries.AsNoTracking()
            .Where(x => x.MatchReviewStatus == BeneficiaryMatchReviewStatus.PendingReview && x.IsActive &&
                        (facilityId == null || x.RegistrationFacilityId == facilityId))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

    public Task<BeneficiaryEntity?> GetByFullCodeAsync(string fullCode, CancellationToken ct) =>
        db.Beneficiaries.AsNoTracking().FirstOrDefaultAsync(x => x.FullCode == fullCode, ct);

    public async Task<IReadOnlyList<BeneficiaryEntity>> SearchByNameAsync(string normalizedFirstName,
        string normalizedLastName, int take, CancellationToken ct) =>
        await db.Beneficiaries.AsNoTracking()
            .Where(x => x.MatchReviewStatus != BeneficiaryMatchReviewStatus.MergedIntoExisting &&
                        x.NormalizedLastName.StartsWith(normalizedLastName) &&
                        x.NormalizedFirstName.StartsWith(normalizedFirstName))
            .OrderBy(x => x.NormalizedLastName)
            .ThenBy(x => x.NormalizedFirstName)
            .ThenBy(x => x.BirthDate)
            .Take(take)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<BeneficiaryEntity> Items, int TotalCount)> GetPagedByFacilityAsync(
        Guid facilityId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Beneficiaries.AsNoTracking()
            .Where(x => x.RegistrationFacilityId == facilityId && x.IsActive);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
