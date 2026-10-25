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

    /// <summary>Next value of the database sequence used for Beneficiary.Code (starts at 1).</summary>
    Task<int> GetNextCodeAsync(CancellationToken cancellationToken);

    /// <summary>Active beneficiaries waiting for a match review, oldest first. facilityId null = all facilities.</summary>
    Task<IReadOnlyList<BeneficiaryEntity>> GetPendingReviewAsync(Guid? facilityId, CancellationToken ct);

    /// <summary>Exact FullCode lookup (unique). Returns null when not found.</summary>
    Task<BeneficiaryEntity?> GetByFullCodeAsync(string fullCode, CancellationToken cancellationToken);

    /// <summary>
    /// Starts-with search on the normalized first and last name, merged duplicates excluded,
    /// ordered by last name, first name, birth date. The caller passes take = limit + 1 to detect "has more".
    /// </summary>
    Task<IReadOnlyList<BeneficiaryEntity>> SearchByNameAsync(string normalizedFirstName,
        string normalizedLastName, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Active beneficiaries registered at the facility, newest first (CreatedAt descending, then Id).
    /// page is 1-based. Returns the page items and the total number of matching rows.
    /// </summary>
    Task<(IReadOnlyList<BeneficiaryEntity> Items, int TotalCount)> GetPagedByFacilityAsync(
        Guid facilityId, int page, int pageSize, CancellationToken cancellationToken);

    Task AddAsync(BeneficiaryEntity beneficiary, CancellationToken cancellationToken);
}
