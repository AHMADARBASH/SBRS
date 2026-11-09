using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

/// <summary>
/// Access to the sensitive profile and its child rows. Never used by search/list features:
/// the profile must not leak into Search, GetAll, PendingReview or MatchCandidates.
/// </summary>
public interface IBeneficiaryProfileRepository
{
    /// <summary>Tracked instance (the save handler calls Update on it); null when no profile exists yet.</summary>
    Task<BeneficiaryProfileEntity?> GetByBeneficiaryIdAsync(Guid beneficiaryId, CancellationToken ct);

    /// <summary>Tracked disability rows of the beneficiary (diff-synced on save).</summary>
    Task<IReadOnlyList<BeneficiaryDisabilityEntity>> GetDisabilitiesAsync(Guid beneficiaryId, CancellationToken ct);

    /// <summary>Tracked chronic-disease rows of the beneficiary (diff-synced on save).</summary>
    Task<IReadOnlyList<BeneficiaryChronicDiseaseEntity>> GetChronicDiseasesAsync(Guid beneficiaryId, CancellationToken ct);

    Task AddAsync(BeneficiaryProfileEntity profile, CancellationToken ct);
    Task AddAsync(BeneficiaryDisabilityEntity disability, CancellationToken ct);
    Task AddAsync(BeneficiaryChronicDiseaseEntity chronicDisease, CancellationToken ct);

    /// <summary>Stages removal; the handler's single SaveChangesAsync commits it.</summary>
    void RemoveDisabilities(IEnumerable<BeneficiaryDisabilityEntity> disabilities);

    /// <summary>Stages removal; the handler's single SaveChangesAsync commits it.</summary>
    void RemoveChronicDiseases(IEnumerable<BeneficiaryChronicDiseaseEntity> chronicDiseases);
}
