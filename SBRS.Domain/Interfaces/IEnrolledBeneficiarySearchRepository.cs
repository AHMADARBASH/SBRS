using SBRS.Domain.Common;
using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

/// <summary>Search restricted to the active roster of one facility.</summary>
public interface IEnrolledBeneficiarySearchRepository
{
    Task<IReadOnlyList<BeneficiaryEntity>> SearchEnrolledAsync(BeneficiarySearchCriteria criteria,
        Guid facilityId, int take, CancellationToken ct);
}
