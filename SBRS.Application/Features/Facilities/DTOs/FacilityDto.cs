namespace SBRS.Application.Features.Facilities.DTOs;

public record FacilityDto(
    Guid Id,
    Guid BranchId,
    Guid AddressId,
    string ArName,
    string EnName,
    string Code,
    bool IsActive,
    Guid typeId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);