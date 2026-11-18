using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

/// <summary>
/// One enrollment of a beneficiary at a facility. A beneficiary is registered once globally
/// (RegistrationFacilityId), but may be enrolled by several facilities. The facility
/// beneficiary list is built from these rows.
/// </summary>
public class FacilityBeneficiaryEntity : BaseEntity
{
    public Guid BeneficiaryId { get; private set; }
    public Guid FacilityId { get; private set; }
    public DateOnly EnrolledOn { get; private set; }
    public bool IsActive { get; private set; } = true;

    private FacilityBeneficiaryEntity() { }   // EF

    public FacilityBeneficiaryEntity(Guid beneficiaryId, Guid facilityId, DateOnly enrolledOn)
    {
        if (beneficiaryId == Guid.Empty) throw new DomainException("Beneficiary is required.");
        if (facilityId == Guid.Empty) throw new DomainException("Facility is required.");

        BeneficiaryId = beneficiaryId;
        FacilityId = facilityId;
        EnrolledOn = enrolledOn;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("The beneficiary is already enrolled at this facility.");

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The beneficiary is already deactivated at this facility.");

        IsActive = false;
    }
}
