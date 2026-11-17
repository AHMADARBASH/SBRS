namespace SBRS.Contracts.Facilities;

// No Code: the facility code is immutable and cannot be changed.
// CategoryId is optional: send null to leave the facility without a category.
public record UpdateFacilityRequest(string ArName, string EnName, Guid areaId, Guid typeId, Guid? CategoryId = null);
