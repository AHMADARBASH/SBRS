using SBRS.Application.Features.EducationTypes.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.EducationTypes.Mappings;

public static class EducationTypeMappings
{
    public static EducationTypeDto ToDto(this EducationTypeEntity e) =>
        new(e.Id, e.ArName, e.EnName);
}
