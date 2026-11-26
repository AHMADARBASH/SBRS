namespace SBRS.Contracts.Doctors;

public sealed record AssignDoctorClinicRequest(Guid DoctorId, Guid ClinicId);
