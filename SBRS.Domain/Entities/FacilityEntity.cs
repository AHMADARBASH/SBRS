using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class FacilityEntity : BaseEntity
{
    public Guid BranchId { get; private set; }
    public BranchEntity Branch { get; private set; } = default!;
    public Guid AddressId { get; private set; }
    public AddressEntity Address { get; private set; } = default!;
    public string ArName { get; private set; } = default!;
    public string EnName { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private FacilityEntity() { }   // EF Core

    public FacilityEntity(Guid branchId, Guid addressId, string arName, string enName, string code)
    {
        BranchId = branchId;
        AddressId = addressId;
        ArName = arName.Trim();
        EnName = enName.Trim();
        Code = code.Trim();
    }

    public void Update(string arName, string enName, string code, Guid addressId)
    {
        if (!IsActive)
            throw new DomainException("A deactivated facility cannot be updated.");

        AddressId = addressId;
        ArName = arName.Trim();
        EnName = enName.Trim();
        Code = code.Trim();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The facility is already deactivated.");

        IsActive = false;
    }
}