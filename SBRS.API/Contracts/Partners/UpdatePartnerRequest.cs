namespace SBRS.Contracts.Partners;

public sealed record UpdatePartnerRequest(Guid Id, string ArName, string EnName, string Code);