namespace SBRS.Contracts.FacilityTypes;

public sealed record UpdateFacilityTypeRequest(Guid Id,string ArName, string EnName, string Code);