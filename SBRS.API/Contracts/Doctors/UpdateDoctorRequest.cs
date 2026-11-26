namespace SBRS.Contracts.Doctors;

public sealed record UpdateDoctorRequest(Guid Id, string FirstName, string LastName);
