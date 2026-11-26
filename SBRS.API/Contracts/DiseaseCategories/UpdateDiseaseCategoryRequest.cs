namespace SBRS.Contracts.DiseaseCategories;

public sealed record UpdateDiseaseCategoryRequest(Guid Id, string ArName, string EnName, string Code);
