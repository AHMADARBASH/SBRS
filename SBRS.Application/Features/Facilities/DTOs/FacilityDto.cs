namespace SBRS.Application.Features.Facilities.DTOs;

public record FacilityDto(
    Guid Id,
    Guid BranchId,
    Guid AreaId,
    string ArName,
    string EnName,
    string Code,
    bool IsActive,
    Guid typeId,
    Guid? CategoryId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);