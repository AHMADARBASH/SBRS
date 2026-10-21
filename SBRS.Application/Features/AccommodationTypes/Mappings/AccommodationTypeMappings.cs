using SBRS.Application.Features.AccommodationTypes.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.AccommodationTypes.Mappings;

public static class AccommodationTypeMappings
{
    public static AccommodationTypeDto ToDto(this AccommodationTypeEntity a) =>
        new(a.Id, a.ArName, a.EnName);
}
