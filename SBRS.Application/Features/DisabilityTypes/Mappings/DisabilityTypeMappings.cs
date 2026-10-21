using SBRS.Application.Features.DisabilityTypes.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.DisabilityTypes.Mappings;

public static class DisabilityTypeMappings
{
    public static DisabilityTypeDto ToDto(this DisabilityTypeEntity d) =>
        new(d.Id, d.ArName, d.EnName);
}
