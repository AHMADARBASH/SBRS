namespace SBRS.Contracts.ClinicTypes;

public sealed record UpdateClinicTypeRequest(Guid Id, string ArName, string EnName, string Code);
