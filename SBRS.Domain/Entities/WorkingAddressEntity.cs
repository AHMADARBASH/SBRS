using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class WorkingAddressEntity : BaseEntity
{
    private WorkingAddressEntity() { }   // EF

    public WorkingAddressEntity(Guid facilityId, Guid addressId, DateOnly workingDate, Guid typeId)
    {
        if (facilityId == Guid.Empty) throw new DomainException("Facility is required.");
        if (addressId == Guid.Empty) throw new DomainException("Address is required.");
        if (typeId == Guid.Empty) throw new DomainException("Address type is required.");
        if (workingDate == default) throw new DomainException("Working date is required.");

        FacilityId = facilityId;
        AddressId = addressId;
        SetValues(workingDate, typeId);
    }

    public Guid FacilityId { get; private set; }            // immutable
    public Guid AddressId { get; private set; }             // immutable
    public DateOnly WorkingDate { get; private set; }
    public Guid TypeId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public AddressTypeEntity Type { get; private set; } = default!;
    public AddressEntity Address { get; private set; } = default!;

    public void Update(DateOnly workingDate, Guid typeId) => SetValues(workingDate, typeId);

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The working address is already deactivated.");

        IsActive = false;
    }

    private void SetValues(DateOnly workingDate, Guid typeId)
    {
        if (typeId == Guid.Empty) throw new DomainException("Address type is required.");
        if (workingDate == default) throw new DomainException("Working date is required.");

        WorkingDate = workingDate;
        TypeId = typeId;
    }
}
