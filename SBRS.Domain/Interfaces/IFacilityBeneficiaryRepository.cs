using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IFacilityBeneficiaryRepository
{
    /// <summary>True when an active enrollment exists for this facility and beneficiary.</summary>
    Task<bool> IsEnrolledAsync(Guid beneficiaryId, Guid facilityId, CancellationToken ct);

    /// <summary>
    /// Tracked enrollment (active or inactive) for the facility/beneficiary pair;
    /// the enroll handler reactivates an inactive row instead of creating a duplicate.
    /// </summary>
    Task<FacilityBeneficiaryEntity?> GetAsync(Guid beneficiaryId, Guid facilityId, CancellationToken ct);

    /// <summary>Tracked enrollments of one beneficiary, used by the merge to move rows to the survivor.</summary>
    Task<IReadOnlyList<FacilityBeneficiaryEntity>> GetByBeneficiaryIdAsync(Guid beneficiaryId, CancellationToken ct);

    /// <summary>Active beneficiary ids among the given ids that are enrolled at this facility (one query, no N+1).</summary>
    Task<IReadOnlySet<Guid>> GetEnrolledBeneficiaryIdsAsync(IReadOnlyCollection<Guid> beneficiaryIds,
        Guid facilityId, CancellationToken ct);

    Task AddAsync(FacilityBeneficiaryEntity enrollment, CancellationToken ct);

    /// <summary>Stages removal; the handler's single SaveChangesAsync commits it.</summary>
    void Remove(FacilityBeneficiaryEntity enrollment);
}
