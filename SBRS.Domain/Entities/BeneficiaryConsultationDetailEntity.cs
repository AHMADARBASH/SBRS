using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class BeneficiaryConsultationDetailEntity : BaseEntity
{
    private BeneficiaryConsultationDetailEntity() { }   // EF

    public BeneficiaryConsultationDetailEntity(Guid beneficiaryConsultationId, Guid diseaseId)
    {
        if (beneficiaryConsultationId == Guid.Empty)
            throw new DomainException("Consultation is required.");
        if (diseaseId == Guid.Empty) throw new DomainException("Disease is required.");

        BeneficiaryConsultationId = beneficiaryConsultationId;
        DiseaseId = diseaseId;
    }

    public Guid BeneficiaryConsultationId { get; private set; }   // immutable
    public Guid DiseaseId { get; private set; }                   // immutable
    public bool IsActive { get; private set; } = true;

    public BeneficiaryConsultationEntity BeneficiaryConsultation { get; private set; } = default!;
    public DiseaseEntity Disease { get; private set; } = default!;

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("The consultation disease is already activated.");

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The consultation disease is already deactivated.");

        IsActive = false;
    }
}
