using SBRS.Domain.Common;
using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IBeneficiaryRepository
{
    Task<BeneficiaryEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>All beneficiaries with this exact MatchKey (one per Disambiguator value).</summary>
    Task<IReadOnlyList<BeneficiaryEntity>> GetByMatchKeyAsync(string matchKey, CancellationToken cancellationToken);

    /// <summary>
    /// Small candidate set for C# scoring: born in the same calendar year (as a date range, so the index
    /// is used) AND sharing the normalized last name, mother name or first name. Capped at 200 rows.
    /// </summary>
    Task<IReadOnlyList<BeneficiaryEntity>> GetProbableCandidatesAsync(BeneficiaryIdentityKey key,
        CancellationToken cancellationToken);

    /// <summary>Highest Disambiguator for this MatchKey plus one (0 when the key is unused).</summary>
    Task<int> GetNextDisambiguatorAsync(string matchKey, CancellationToken ct);

    /// <summary>
    /// Beneficiaries holding any of the given identity type + document-number pairs
    /// (unregistered empty values ignored). These are 100% match candidates:
    /// used by registration and match review, excluding the record under review.
    /// </summary>
    Task<IReadOnlyList<BeneficiaryEntity>> GetByExactIdentityAsync(
        IReadOnlyCollection<(Guid IdentityTypeId, string Value)> identities,
        Guid? excludeBeneficiaryId, CancellationToken ct);

    /// <summary>
    /// Places a row lock (SELECT ... FOR UPDATE) on the facility so registrations of that
    /// facility are serialized: the per-facility code sequence cannot race between saves.
    /// </summary>
    Task LockRegistrationFacilityAsync(Guid facilityId, CancellationToken ct);

    /// <summary>
    /// Highest Code among the beneficiaries registered at the given facility, or 0 when none.
    /// Beneficiaries are never hard-deleted (they are deactivated instead), so the max Code
    /// is always available and a number is never reused; a hard delete would allow code reuse.
    /// </summary>
    Task<int> GetMaxCodeForFacilityAsync(Guid facilityId, CancellationToken ct);

    /// <summary>Active beneficiaries waiting for a match review, oldest first. facilityId null = all facilities.</summary>
    Task<IReadOnlyList<BeneficiaryEntity>> GetPendingReviewAsync(Guid? facilityId, CancellationToken ct);

    /// <summary>Exact FullCode lookup (unique). Returns null when not found.</summary>
    Task<BeneficiaryEntity?> GetByFullCodeAsync(string fullCode, CancellationToken cancellationToken);

    /// <summary>
    /// Cross-facility search: every provided key is applied with AND. Names are starts-with
    /// on the normalized columns, FullCode is exact (case-insensitive) and resolves a merged
    /// duplicate to its survivor, and birth date is an exact date match. Merged duplicates are
    /// excluded and the ordering is last name, first name, birth date. The caller passes
    /// take = limit + 1 to detect "has more".
    /// </summary>
    Task<IReadOnlyList<BeneficiaryEntity>> SearchAsync(BeneficiarySearchCriteria criteria, int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Active beneficiaries enrolled at the facility, newest enrollment first (EnrolledOn descending,
    /// then beneficiary CreatedAt descending). page is 1-based. Returns the page items and the
    /// total number of matching rows computed on the same enrollment join.
    /// </summary>
    Task<(IReadOnlyList<BeneficiaryEntity> Items, int TotalCount)> GetPagedByFacilityAsync(
        Guid facilityId, int page, int pageSize, CancellationToken cancellationToken);

    Task AddAsync(BeneficiaryEntity beneficiary, CancellationToken cancellationToken);
}
