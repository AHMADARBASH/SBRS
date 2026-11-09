using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

/// <summary>
/// Links a beneficiary to one chronic disease (lookup rows are never deleted, so exist-check only).
/// Rows are diff-synced by the profile handler; an easy addition for future years is the same row shape.
/// </summary>
public class BeneficiaryChronicDiseaseEntity : BaseEntity
{
    private BeneficiaryChronicDiseaseEntity() { }   // EF

    public BeneficiaryChronicDiseaseEntity(Guid beneficiaryId, Guid chronicDiseaseId)
    {
        if (beneficiaryId == Guid.Empty) throw new DomainException("Beneficiary is required.");
        if (chronicDiseaseId == Guid.Empty) throw new DomainException("Chronic disease is required.");

        BeneficiaryId = beneficiaryId;
        ChronicDiseaseId = chronicDiseaseId;
    }

    public Guid BeneficiaryId { get; private set; }
    public Guid ChronicDiseaseId { get; private set; }
}
