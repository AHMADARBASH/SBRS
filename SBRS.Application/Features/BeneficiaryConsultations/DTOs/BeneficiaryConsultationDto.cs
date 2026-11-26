using SBRS.Application.Features.Diseases.DTOs;

namespace SBRS.Application.Features.BeneficiaryConsultations.DTOs;

public sealed record BeneficiaryConsultationDto(
    Guid Id,
    Guid VisitId,
    DateTime ConsultationDate,
    Guid ClinicId,
    string ClinicTypeCode,
    string ClinicArName,
    string ClinicEnName,
    Guid DoctorId,
    string DoctorFirstName,
    string DoctorLastName,
    IReadOnlyList<DiseaseDto> Diseases);
