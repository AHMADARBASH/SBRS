using SBRS.Application.Common.DTOs;

namespace SBRS.Application.Features.Facilities.DTOs;

public record FacilityDto(
    Guid Id,
    Guid BranchId,
    Guid AreaId,
    string ArName,
    string EnName,
    string Code,
    bool IsActive,
    Guid TypeId,
    Guid? CategoryId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    BranchSummaryDto Branch,
    AreaSummaryDto Area,
    FacilityTypeSummaryDto Type,
    FacilityCategorySummaryDto? Category);
