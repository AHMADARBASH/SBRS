using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class BeneficiaryRepository(AppDbContext db) : IBeneficiaryRepository, IEnrolledBeneficiarySearchRepository
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

    // The review belongs to the facility where the record was registered (it owns the code and
    // the duplicate), so this stays on RegistrationFacilityId and is not driven by enrollments.
    public async Task<IReadOnlyList<BeneficiaryEntity>> GetPendingReviewAsync(Guid? facilityId, CancellationToken ct) =>
        await db.Beneficiaries.AsNoTracking()
            .Where(x => x.MatchReviewStatus == BeneficiaryMatchReviewStatus.PendingReview && x.IsActive &&
                        (facilityId == null || x.RegistrationFacilityId == facilityId))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

    public Task<BeneficiaryEntity?> GetByFullCodeAsync(string fullCode, CancellationToken ct) =>
        db.Beneficiaries.AsNoTracking().FirstOrDefaultAsync(x => x.FullCode == fullCode, ct);

    private IQueryable<BeneficiaryEntity> SearchQuery(BeneficiarySearchCriteria criteria)
    {
        var query = db.Beneficiaries.AsNoTracking()
            .Where(x => x.MatchReviewStatus != BeneficiaryMatchReviewStatus.MergedIntoExisting);

        if (criteria.FullCode is { } fullCode)
        {
            // Exact code, or the survivor a merged duplicate with that code was merged into.
            // FullCode is citext, so the comparison is case-insensitive on PostgreSQL.
            query = query.Where(x =>
                x.FullCode == fullCode ||
                db.Beneficiaries.Any(m =>
                    m.FullCode == fullCode &&
                    m.MatchReviewStatus == BeneficiaryMatchReviewStatus.MergedIntoExisting &&
                    m.MergedIntoBeneficiaryId == x.Id));
        }

        // Every provided key narrows the same query (AND); null keys are ignored.
        if (criteria.NormalizedFirstName is { } firstName)
            query = query.Where(x => x.NormalizedFirstName.StartsWith(firstName));
        if (criteria.NormalizedFatherName is { } fatherName)
            query = query.Where(x => x.NormalizedFatherName.StartsWith(fatherName));
        if (criteria.NormalizedLastName is { } lastName)
            query = query.Where(x => x.NormalizedLastName.StartsWith(lastName));
        if (criteria.NormalizedMotherName is { } motherName)
            query = query.Where(x => x.NormalizedMotherName.StartsWith(motherName));
        if (criteria.BirthDate is { } birthDate)
            query = query.Where(x => x.BirthDate == birthDate);

        return query;
    }

    private static IQueryable<BeneficiaryEntity> OrderSearch(IQueryable<BeneficiaryEntity> query) =>
        query.OrderBy(x => x.NormalizedLastName)
             .ThenBy(x => x.NormalizedFirstName)
             .ThenBy(x => x.BirthDate);

    public async Task<IReadOnlyList<BeneficiaryEntity>> SearchAsync(BeneficiarySearchCriteria criteria, int take,
        CancellationToken ct) =>
        await OrderSearch(SearchQuery(criteria)).Take(take).ToListAsync(ct);

    public async Task<IReadOnlyList<BeneficiaryEntity>> SearchEnrolledAsync(BeneficiarySearchCriteria criteria,
        Guid facilityId, int take, CancellationToken ct) =>
        await OrderSearch(SearchQuery(criteria)
                // same active-roster rule as GetAll: active enrollment of this facility
                .Where(x => x.IsActive && db.FacilityBeneficiaries.Any(e =>
                    e.BeneficiaryId == x.Id && e.FacilityId == facilityId && e.IsActive)))
            .Take(take)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<BeneficiaryEntity> Items, int TotalCount)> GetPagedByFacilityAsync(
        Guid facilityId, int page, int pageSize, CancellationToken ct)
    {
        // The roster is driven by enrollments, not by RegistrationFacilityId: a person enrolled
        // at two facilities appears in both lists. Inactive enrollments and inactive/merged
        // beneficiaries stay hidden. One join, no per-row queries.
        var query =
            from enrollment in db.FacilityBeneficiaries.AsNoTracking()
            join beneficiary in db.Beneficiaries.AsNoTracking()
                on enrollment.BeneficiaryId equals beneficiary.Id
            where enrollment.FacilityId == facilityId && enrollment.IsActive && beneficiary.IsActive
            select new { Enrollment = enrollment, Beneficiary = beneficiary };

        // computed on the same joined query, so the total matches what the pages contain
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Enrollment.EnrolledOn)
            .ThenByDescending(x => x.Beneficiary.CreatedAt)
            .ThenBy(x => x.Enrollment.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Beneficiary)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
