using SBRS.Application.Features.FacilityTypes.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.FacilityTypes.Mappings;

public static class FacilityTypeMappings
{
    public static FacilityTypeDto ToDto(this FacilityTypeEntity t) =>
        new(t.Id, t.ArName, t.EnName, t.Code);
}