namespace SBRS.Contracts.Clinics;

public sealed record AddClinicRequest(Guid FacilityId, Guid ClinicTypeId);
