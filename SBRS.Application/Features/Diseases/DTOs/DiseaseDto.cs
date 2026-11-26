namespace SBRS.Application.Features.Diseases.DTOs;

public sealed record DiseaseDto(
    Guid Id,
    string ArName,
    string EnName,
    string Code,
    Guid DiseaseCategoryId,
    string DiseaseCategoryCode,
    string DiseaseCategoryArName,
    string DiseaseCategoryEnName);
