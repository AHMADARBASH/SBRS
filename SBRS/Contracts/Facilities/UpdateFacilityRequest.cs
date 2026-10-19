namespace SBRS.Contracts.Facilities;

public record UpdateFacilityRequest(string ArName, string EnName, string Code,Guid addressId,Guid typeId);