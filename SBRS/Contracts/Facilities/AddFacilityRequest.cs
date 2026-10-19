namespace SBRS.Contracts.Facilities;

public record AddFacilityRequest(string ArName, string EnName, string Code,Guid branchId,Guid addressId,Guid typeId,Guid partnerId,DateOnly StartDate,string? ProjectCode);