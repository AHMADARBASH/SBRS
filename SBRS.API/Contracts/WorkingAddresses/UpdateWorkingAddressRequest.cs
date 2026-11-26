namespace SBRS.Contracts.WorkingAddresses;

public sealed record UpdateWorkingAddressRequest(Guid Id, DateOnly? WorkingDate, Guid TypeId);
