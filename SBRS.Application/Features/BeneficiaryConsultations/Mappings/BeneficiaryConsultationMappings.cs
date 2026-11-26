using SBRS.Application.Features.BeneficiaryConsultations.DTOs;
using SBRS.Application.Features.Diseases.DTOs;
using SBRS.Application.Features.Diseases.Mappings;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.BeneficiaryConsultations.Mappings;

public static class BeneficiaryConsultationMappings
{
    public static BeneficiaryConsultationDto ToDto(this BeneficiaryConsultationEntity e) =>
        e.ToDto(e.Clinic, e.Doctor);

    public static BeneficiaryConsultationDto ToDto(this BeneficiaryConsultationEntity e,
        ClinicEntity clinic, DoctorEntity doctor) =>
        e.ToDto(clinic, doctor, e.ConsultationDetails.Where(d => d.IsActive)
            .Select(d => d.Disease.ToDto())
            .OrderBy(d => d.Code)
            .ToList());

    public static BeneficiaryConsultationDto ToDto(this BeneficiaryConsultationEntity e,
        ClinicEntity clinic, DoctorEntity doctor, IReadOnlyList<DiseaseDto> diseases) =>
        new(e.Id, e.VisitId, e.ConsultationDate,
            clinic.Id, clinic.ClinicType.Code, clinic.ClinicType.ArName, clinic.ClinicType.EnName,
            doctor.Id, doctor.FirstName, doctor.LastName, diseases);
}
