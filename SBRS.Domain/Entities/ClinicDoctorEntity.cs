using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class ClinicDoctorEntity : BaseEntity
{
    private ClinicDoctorEntity() { }   // EF

    public ClinicDoctorEntity(Guid clinicId, Guid doctorId)
    {
        if (clinicId == Guid.Empty) throw new DomainException("Clinic is required.");
        if (doctorId == Guid.Empty) throw new DomainException("Doctor is required.");

        ClinicId = clinicId;
        DoctorId = doctorId;
    }

    public Guid ClinicId { get; private set; }
    public Guid DoctorId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ClinicEntity Clinic { get; private set; } = default!;
    public DoctorEntity Doctor { get; private set; } = default!;

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The clinic assignment is already deactivated.");

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("The clinic assignment is already activated.");

        IsActive = true;
    }
}
