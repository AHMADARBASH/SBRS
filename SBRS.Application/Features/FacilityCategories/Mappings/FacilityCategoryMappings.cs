using SBRS.Application.Features.FacilityCategories.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.FacilityCategories.Mappings;

public static class FacilityCategoryMappings
{
    public static FacilityCategoryDto ToDto(this FacilityCategoryEntity c) =>
        new(c.Id, c.ArName, c.EnName, c.Code);
}
