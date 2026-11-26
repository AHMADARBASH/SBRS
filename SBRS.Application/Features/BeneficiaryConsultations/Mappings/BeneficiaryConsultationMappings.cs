using SBRS.Application.Features.BeneficiaryConsultations.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.BeneficiaryConsultations.Mappings;

public static class BeneficiaryConsultationMappings
{
    public static BeneficiaryConsultationDto ToDto(this BeneficiaryConsultationEntity e) =>
        e.ToDto(e.Clinic, e.Doctor);

    public static BeneficiaryConsultationDto ToDto(this BeneficiaryConsultationEntity e,
        ClinicEntity clinic, DoctorEntity doctor) =>
        new(e.Id, e.VisitId, e.ConsultationDate,
            clinic.Id, clinic.ClinicType.Code, clinic.ClinicType.ArName, clinic.ClinicType.EnName,
            doctor.Id, doctor.FirstName, doctor.LastName);
}
