namespace SBRS.Contracts.WorkingAddresses;

public sealed record NewWorkingAddressRequest(Guid AreaId, string ArName, string EnName);

public sealed record AddWorkingAddressRequest(
    DateOnly? WorkingDate,
    Guid TypeId,
    Guid? AddressId,
    NewWorkingAddressRequest? NewAddress);
