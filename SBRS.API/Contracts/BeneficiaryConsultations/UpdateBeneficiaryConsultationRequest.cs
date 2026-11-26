namespace SBRS.Contracts.BeneficiaryConsultations;

public sealed record UpdateBeneficiaryConsultationRequest(
    Guid Id,
    DateTime ConsultationDate,
    Guid ClinicId,
    Guid DoctorId,
    IReadOnlyList<Guid>? DiseaseIds);
