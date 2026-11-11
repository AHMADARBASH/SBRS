namespace SBRS.Contracts.Facilities;

// No Code: the system generates the facility code ({CityCCode}-{FacilityTypeCode}-{Sequence}).
public record AddFacilityRequest(string ArName, string EnName, Guid branchId, Guid addressId, Guid typeId, Guid partnerId, DateOnly StartDate, string? ProjectCode);
