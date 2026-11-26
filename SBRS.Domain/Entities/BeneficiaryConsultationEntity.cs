using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class BeneficiaryConsultationEntity : BaseEntity
{
    private BeneficiaryConsultationEntity() { }   // EF

    public BeneficiaryConsultationEntity(DateTime consultationDate, Guid visitId,
        Guid clinicId, Guid doctorId)
    {
        if (visitId == Guid.Empty) throw new DomainException("Visit is required.");
        if (clinicId == Guid.Empty) throw new DomainException("Clinic is required.");
        if (doctorId == Guid.Empty) throw new DomainException("Doctor is required.");

        VisitId = visitId;
        SetValues(consultationDate, clinicId, doctorId);
    }

    public DateTime ConsultationDate { get; private set; }
    public Guid VisitId { get; private set; }               // immutable
    public Guid ClinicId { get; private set; }
    public Guid DoctorId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public VisitEntity Visit { get; private set; } = default!;
    public ClinicEntity Clinic { get; private set; } = default!;
    public DoctorEntity Doctor { get; private set; } = default!;

    public void Update(DateTime consultationDate, Guid clinicId, Guid doctorId) =>
        SetValues(consultationDate, clinicId, doctorId);

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The consultation is already deactivated.");

        IsActive = false;
    }

    private void SetValues(DateTime consultationDate, Guid clinicId, Guid doctorId)
    {
        if (clinicId == Guid.Empty) throw new DomainException("Clinic is required.");
        if (doctorId == Guid.Empty) throw new DomainException("Doctor is required.");

        ClinicId = clinicId;
        DoctorId = doctorId;
        ConsultationDate = ToUtc(consultationDate);
    }

    // Npgsql requires a UTC DateTime for timestamptz. An unspecified kind is treated as UTC.
    private static DateTime ToUtc(DateTime value)
    {
        if (value == default) throw new DomainException("Consultation date is required.");

        return value.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            DateTimeKind.Utc => value,
            _ => value.ToUniversalTime()
        };
    }
}
