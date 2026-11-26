using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class VisitEntity : BaseEntity
{
    private VisitEntity() { }   // EF

    public VisitEntity(Guid facilityId, Guid beneficiaryId, DateOnly visitDate,
        decimal? weight, decimal? height, bool? isPregnant)
    {
        if (facilityId == Guid.Empty) throw new DomainException("Facility is required.");
        if (beneficiaryId == Guid.Empty) throw new DomainException("Beneficiary is required.");

        FacilityId = facilityId;
        BeneficiaryId = beneficiaryId;
        SetValues(visitDate, weight, height, isPregnant);
    }

    public Guid FacilityId { get; private set; }              // immutable
    public Guid BeneficiaryId { get; private set; }           // immutable
    public DateOnly VisitDate { get; private set; }
    public decimal? Weight { get; private set; }
    public decimal? Height { get; private set; }
    public bool? IsPregnant { get; private set; }
    public bool IsActive { get; private set; } = true;

    public BeneficiaryEntity Beneficiary { get; private set; } = default!;

    public void Update(DateOnly visitDate, decimal? weight, decimal? height, bool? isPregnant) =>
        SetValues(visitDate, weight, height, isPregnant);

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The visit is already deactivated.");

        IsActive = false;
    }

    private void SetValues(DateOnly visitDate, decimal? weight, decimal? height, bool? isPregnant)
    {
        if (visitDate == default) throw new DomainException("Visit date is required.");
        if (weight is < 0.5m or > 200m)
            throw new DomainException("Weight must be between 0.5 and 200 kg.");
        if (height is < 20m or > 250m)
            throw new DomainException("Height must be between 20 and 250 cm.");

        VisitDate = visitDate;
        Weight = weight;
        Height = height;
        IsPregnant = isPregnant;
    }
}
