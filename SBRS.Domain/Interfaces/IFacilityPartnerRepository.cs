using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IFacilityPartnerRepository
{
    /// <summary>The open assignment (EndDate is null), tracked for update.</summary>
    Task<FacilityPartnerEntity?> GetCurrentAsync(Guid facilityId, CancellationToken ct);

    /// <summary>Full history with the partner loaded, newest first.</summary>
    Task<IReadOnlyList<FacilityPartnerEntity>> GetByFacilityAsync(Guid facilityId, CancellationToken ct);

    Task AddAsync(FacilityPartnerEntity entry, CancellationToken ct);
}