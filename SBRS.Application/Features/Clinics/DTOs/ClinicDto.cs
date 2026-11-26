namespace SBRS.Application.Features.Clinics.DTOs;

public sealed record ClinicDto(
    Guid Id,
    Guid FacilityId,
    Guid ClinicTypeId,
    string TypeCode,
    string ArName,
    string EnName,
    bool IsActive);
