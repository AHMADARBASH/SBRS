namespace SBRS.Application.Features.Facilities.DTOs;

public record FacilityDto(
    Guid Id,
    Guid BranchId,
    Guid AddressId,
    string ArName,
    string EnName,
    string Code,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);