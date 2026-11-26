namespace SBRS.Application.Features.Doctors.DTOs;

public sealed record DoctorClinicDto(Guid ClinicId, string TypeCode, string ArName, string EnName);

public sealed record DoctorDto(
    Guid Id,
    Guid FacilityId,
    string FirstName,
    string LastName,
    bool IsActive,
    IReadOnlyList<DoctorClinicDto> Clinics);
