using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IClinicDoctorRepository
{
    /// <summary>Tracked link (active or inactive); used to reactivate an existing assignment.</summary>
    Task<ClinicDoctorEntity?> GetAsync(Guid clinicId, Guid doctorId, CancellationToken ct);

    /// <summary>Tracked active links of a doctor; deactivated together with the doctor.</summary>
    Task<IReadOnlyList<ClinicDoctorEntity>> GetActiveByDoctorAsync(Guid doctorId, CancellationToken ct);

    Task AddAsync(ClinicDoctorEntity link, CancellationToken ct);
}
