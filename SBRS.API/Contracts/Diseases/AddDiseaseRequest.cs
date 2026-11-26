namespace SBRS.Contracts.Diseases;

public sealed record AddDiseaseRequest(string ArName, string EnName, string Code, Guid DiseaseCategoryId);
