using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IBeneficiaryIdentityRepository
{
    /// <summary>All identities of a beneficiary with their type loaded, oldest first.</summary>
    Task<IReadOnlyList<BeneficiaryIdentityEntity>> GetByBeneficiaryIdAsync(Guid beneficiaryId,
        CancellationToken cancellationToken);

    /// <summary>
    /// The primary identity (the oldest row) of each beneficiary, with the type loaded.
    /// Beneficiaries without an identity are simply absent from the dictionary.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, BeneficiaryIdentityEntity>> GetPrimaryByBeneficiaryIdsAsync(
        IReadOnlyCollection<Guid> beneficiaryIds, CancellationToken cancellationToken);

    /// <summary>All identities of every listed beneficiary (oldest first), with the type loaded.</summary>
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<BeneficiaryIdentityEntity>>> GetByBeneficiaryIdsAsync(
        IReadOnlyCollection<Guid> beneficiaryIds, CancellationToken cancellationToken);

    Task AddAsync(BeneficiaryIdentityEntity identity, CancellationToken cancellationToken);

    // Tracked primary identity (the oldest row) of a beneficiary, so it can be updated in place.
    // Returns null when the beneficiary has no identity row yet.
    Task<BeneficiaryIdentityEntity?> GetPrimaryForUpdateAsync(Guid beneficiaryId,
        CancellationToken cancellationToken);
}
