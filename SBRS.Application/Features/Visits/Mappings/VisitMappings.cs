using SBRS.Application.Features.Visits.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.Visits.Mappings;

public static class VisitMappings
{
    public static VisitDto ToDto(this VisitEntity v) => v.ToDto(v.Beneficiary);

    public static VisitDto ToDto(this VisitEntity v, BeneficiaryEntity beneficiary) =>
        new(v.Id, v.FacilityId, v.BeneficiaryId,
            beneficiary.FullCode, beneficiary.FirstName, beneficiary.LastName,
            v.VisitDate, v.Weight, v.Height, v.IsPregnant);
}
