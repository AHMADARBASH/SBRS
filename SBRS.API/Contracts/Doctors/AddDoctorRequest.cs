namespace SBRS.Contracts.Doctors;

public sealed record AddDoctorRequest(Guid FacilityId, string FirstName, string LastName);
