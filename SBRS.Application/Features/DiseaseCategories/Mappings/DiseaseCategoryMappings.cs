using SBRS.Application.Features.DiseaseCategories.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.DiseaseCategories.Mappings;

public static class DiseaseCategoryMappings
{
    public static DiseaseCategoryDto ToDto(this DiseaseCategoryEntity e) =>
        new(e.Id, e.ArName, e.EnName, e.Code);
}
