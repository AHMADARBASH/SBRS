using SBRS.Application.Features.ClinicTypes.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.ClinicTypes.Mappings;

public static class ClinicTypeMappings
{
    public static ClinicTypeDto ToDto(this ClinicTypeEntity e) =>
        new(e.Id, e.ArName, e.EnName, e.Code);
}
