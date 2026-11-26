using SBRS.Application.Features.Diseases.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.Diseases.Mappings;

public static class DiseaseMappings
{
    public static DiseaseDto ToDto(this DiseaseEntity e) =>
        new(e.Id, e.ArName, e.EnName, e.Code,
            e.DiseaseCategoryId, e.DiseaseCategory.Code, e.DiseaseCategory.ArName, e.DiseaseCategory.EnName);

    public static DiseaseDto ToDto(this DiseaseEntity e, DiseaseCategoryEntity category) =>
        new(e.Id, e.ArName, e.EnName, e.Code,
            category.Id, category.Code, category.ArName, category.EnName);
}
