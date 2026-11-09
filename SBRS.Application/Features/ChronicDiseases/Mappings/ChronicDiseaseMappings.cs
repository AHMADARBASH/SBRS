using SBRS.Application.Features.ChronicDiseases.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.ChronicDiseases.Mappings;

public static class ChronicDiseaseMappings
{
    public static ChronicDiseaseDto ToDto(this ChronicDiseaseEntity disease) =>
        new(disease.Id, disease.Code, disease.ArName, disease.EnName);
}
