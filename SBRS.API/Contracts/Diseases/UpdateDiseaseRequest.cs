namespace SBRS.Contracts.Diseases;

public sealed record UpdateDiseaseRequest(
    Guid Id, string ArName, string EnName, string Code, Guid DiseaseCategoryId);
