using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class ClinicEntity : BaseEntity
{
    private ClinicEntity() { }   // EF

    public ClinicEntity(Guid facilityId, Guid clinicTypeId)
    {
        if (facilityId == Guid.Empty) throw new DomainException("Facility is required.");
        if (clinicTypeId == Guid.Empty) throw new DomainException("Clinic type is required.");

        FacilityId = facilityId;
        ClinicTypeId = clinicTypeId;
    }

    public Guid FacilityId { get; private set; }            // immutable
    public Guid ClinicTypeId { get; private set; }          // immutable
    public ClinicTypeEntity ClinicType { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The clinic is already deactivated.");

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("The clinic is already activated.");

        IsActive = true;
    }
}
