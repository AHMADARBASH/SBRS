using SBRS.Application.Features.Clinics.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.Clinics.Mappings;

public static class ClinicMappings
{
    public static ClinicDto ToDto(this ClinicEntity e) =>
        new(e.Id, e.FacilityId, e.ClinicTypeId,
            e.ClinicType.Code, e.ClinicType.ArName, e.ClinicType.EnName, e.IsActive);

    public static ClinicDto ToDto(this ClinicEntity e, ClinicTypeEntity type) =>
        new(e.Id, e.FacilityId, e.ClinicTypeId,
            type.Code, type.ArName, type.EnName, e.IsActive);
}
