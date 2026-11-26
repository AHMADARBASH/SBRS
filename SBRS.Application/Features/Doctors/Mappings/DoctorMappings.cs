using SBRS.Application.Features.Doctors.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.Doctors.Mappings;

public static class DoctorMappings
{
    public static DoctorDto ToDto(this DoctorEntity e) =>
        new(e.Id, e.FacilityId, e.FirstName, e.LastName, e.IsActive,
            e.ClinicDoctors.Where(l => l.IsActive)
                .Select(l => new DoctorClinicDto(
                    l.ClinicId, l.Clinic.ClinicType.Code, l.Clinic.ClinicType.ArName, l.Clinic.ClinicType.EnName))
                .ToList());
}
