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

    Task AddAsync(BeneficiaryIdentityEntity identity, CancellationToken cancellationToken);
}
