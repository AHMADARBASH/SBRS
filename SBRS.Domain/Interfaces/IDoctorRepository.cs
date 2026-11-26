using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IDoctorRepository
{
    /// <summary>Scoped, tracked (update/deactivate/activate).</summary>
    Task<DoctorEntity?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Scoped read with the active clinic links and their types loaded.</summary>
    Task<DoctorEntity?> GetByIdWithClinicsAsync(Guid id, CancellationToken ct);

    /// <summary>Scoped list with the active clinic links loaded.</summary>
    Task<IReadOnlyList<DoctorEntity>> GetAllAsync(Guid? facilityId, Guid? clinicId,
        bool includeInactive, CancellationToken ct);

    Task AddAsync(DoctorEntity doctor, CancellationToken ct);
}
