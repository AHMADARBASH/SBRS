namespace SBRS.Contracts.Facilities;

// No Code: the facility code is immutable and cannot be changed.
public record UpdateFacilityRequest(string ArName, string EnName, Guid addressId, Guid typeId);
