using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

/// <summary>
/// One disability of a beneficiary (which type it is and why it happened).
/// Rows exist only while the profile has IsDisabled = true; the handler diff-syncs them.
/// </summary>
public class BeneficiaryDisabilityEntity : BaseEntity
{
    private BeneficiaryDisabilityEntity() { }   // EF

    public BeneficiaryDisabilityEntity(Guid beneficiaryId, Guid disabilityTypeId, Guid reasonId)
    {
        if (beneficiaryId == Guid.Empty) throw new DomainException("Beneficiary is required.");
        if (disabilityTypeId == Guid.Empty) throw new DomainException("Disability type is required.");
        if (reasonId == Guid.Empty) throw new DomainException("Disability reason is required.");

        BeneficiaryId = beneficiaryId;
        DisabilityTypeId = disabilityTypeId;
        ReasonId = reasonId;
    }

    public Guid BeneficiaryId { get; private set; }
    public Guid DisabilityTypeId { get; private set; }
    public Guid ReasonId { get; private set; }

    public void Update(Guid reasonId)
    {
        if (reasonId == Guid.Empty) throw new DomainException("Disability reason is required.");
        ReasonId = reasonId;
    }
}
