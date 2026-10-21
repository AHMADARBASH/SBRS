namespace SBRS.Contracts.Nationalities;

public sealed record UpdateNationalityRequest(Guid Id, string ArName, string EnName);
