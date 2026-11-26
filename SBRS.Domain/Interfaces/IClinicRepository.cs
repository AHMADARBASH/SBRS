using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IClinicRepository
{
    /// <summary>Scoped, tracked (deactivate/activate) and with the type loaded.</summary>
    Task<ClinicEntity?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Scoped list; a null facilityId means every facility the user may see.</summary>
    Task<IReadOnlyList<ClinicEntity>> GetAllAsync(Guid? facilityId, bool includeInactive, CancellationToken ct);

    /// <summary>Unique pair lookup including inactive clinics (used to reject duplicates).</summary>
    Task<ClinicEntity?> GetByFacilityAndTypeAsync(Guid facilityId, Guid clinicTypeId, CancellationToken ct);

    Task<bool> HasActiveDoctorLinksAsync(Guid clinicId, CancellationToken ct);

    Task AddAsync(ClinicEntity clinic, CancellationToken ct);
}
