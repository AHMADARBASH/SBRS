namespace SBRS.Contracts.Facilities;

// No Code: the system generates the facility code ({CityCCode}-{FacilityTypeCode}-{Sequence}).
public record AddFacilityRequest(string ArName, string EnName, Guid branchId, Guid areaId, Guid typeId, Guid partnerId, DateOnly StartDate, string? ProjectCode, Guid? CategoryId = null);
