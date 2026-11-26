namespace SBRS.Contracts.BeneficiaryConsultations;

public sealed record AddBeneficiaryConsultationRequest(
    DateTime ConsultationDate,
    Guid VisitId,
    Guid ClinicId,
    Guid DoctorId);
