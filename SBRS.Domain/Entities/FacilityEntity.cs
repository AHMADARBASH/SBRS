using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class FacilityEntity : BaseEntity
{
    public Guid BranchId { get; private set; }
    public BranchEntity Branch { get; private set; } = default!;
    public Guid AddressId { get; private set; }
    public AddressEntity Address { get; private set; } = default!;
    public Guid TypeId { get; private set; }
    public FacilityTypeEntity FacilityType { get; private set; } = default!;
    public Guid? CategoryId { get; private set; }
    public FacilityCategoryEntity? Category { get; private set; }
    public Guid PartnerId { get; private set; }
    public PartnerEntity Partner { get; private set; } = default!;
    public string ArName { get; private set; } = default!;
    public string EnName { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private FacilityEntity() { }   // EF Core

    public FacilityEntity(Guid branchId, Guid addressId, Guid typeId,Guid partnerId, string arName, string enName, string code, Guid? categoryId = null)
    {
        if (branchId == Guid.Empty) throw new DomainException("Branch is required.");
        if (addressId == Guid.Empty) throw new DomainException("Address is required.");
        if (typeId == Guid.Empty) throw new DomainException("Facility type is required.");
        if (partnerId == Guid.Empty) throw new DomainException("Partner is required.");
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Facility code is required.");
        if (categoryId == Guid.Empty) throw new DomainException("Facility category is invalid.");

        BranchId = branchId;
        AddressId = addressId;
        ArName = arName.Trim();
        EnName = enName.Trim();
        Code = code.Trim();
        TypeId = typeId;
        PartnerId = partnerId;
        CategoryId = categoryId;
    }

    /// <summary>
    /// The code is generated at creation and is immutable: it is never recomputed
    /// when the facility is edited, even if its type, address or city changes,
    /// because beneficiary FullCodes are built from it.
    /// </summary>
    public void Update(string arName, string enName, Guid addressId, Guid typeId, Guid? categoryId)
    {
        if (!IsActive)
            throw new DomainException("A deactivated facility cannot be updated.");
        if (categoryId == Guid.Empty)
            throw new DomainException("Facility category is invalid.");

        AddressId = addressId;
        ArName = arName.Trim();
        EnName = enName.Trim();
        TypeId = typeId;
        CategoryId = categoryId;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The facility is already deactivated.");

        IsActive = false;
    }

    public void ChangePartner(Guid partnerId)
    {
        if (!IsActive)
            throw new DomainException("A deactivated facility cannot change its partner.");

        PartnerId = partnerId;
    }
}