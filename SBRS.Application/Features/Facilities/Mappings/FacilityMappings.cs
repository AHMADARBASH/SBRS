using SBRS.Application.Common.DTOs;
using SBRS.Application.Features.Facilities.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.Facilities.Mappings;

public static class FacilityMappings
{
    public static FacilityDto ToDto(this FacilityEntity e)
        => new(
            e.Id,
            e.BranchId,
            e.AreaId,
            e.ArName,
            e.EnName,
            e.Code,
            e.IsActive,
            e.TypeId,
            e.CategoryId,
            e.CreatedAt,
            e.UpdatedAt,
            new BranchSummaryDto(e.Branch.Id, e.Branch.Code, e.Branch.ArName, e.Branch.EnName, e.Branch.IsActive),
            new AreaSummaryDto(e.Area.Id, e.Area.Code, e.Area.ArName, e.Area.EnName),
            new FacilityTypeSummaryDto(e.FacilityType.Id, e.FacilityType.Code, e.FacilityType.ArName, e.FacilityType.EnName),
            e.Category is null
                ? null
                : new FacilityCategorySummaryDto(
                    e.Category.Id, e.Category.Code, e.Category.ArName, e.Category.EnName));
}
