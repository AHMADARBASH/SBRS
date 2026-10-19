using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class FacilityPartnerEntity : BaseEntity
{
    public Guid FacilityId { get; private set; }
    public FacilityEntity Facility { get; private set; } = default!;
    public Guid PartnerId { get; private set; }
    public PartnerEntity Partner { get; private set; } = default!;
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? ProjectCode { get; private set; } = string.Empty;

    public bool IsCurrent => EndDate is null;

    private FacilityPartnerEntity() { }   // EF

    public FacilityPartnerEntity(FacilityEntity facility, PartnerEntity partner, DateOnly startDate, string projectCode)
    {

        Facility = facility;
        FacilityId = facility.Id;
        Partner = partner;
        PartnerId = partner.Id;
        StartDate = startDate;
        ProjectCode = projectCode.Trim() ?? string.Empty;
    }

    public void Close(DateOnly endDate)
    {
        if (EndDate is not null)
            throw new DomainException("This partner assignment is already closed.");

        if (endDate < StartDate)
            throw new DomainException("End date cannot be before the start date.");

        EndDate = endDate;
    }
}