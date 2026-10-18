using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class AddressEntity : BaseEntity
{
    public Guid AreaId { get; private set; }
    public string ArName { get; private set; } = default!;
    public string EnName { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private AddressEntity() { }   // EF Core

    public AddressEntity(Guid areaId, string arName, string enName)
    {
        AreaId = areaId;
        ArName = arName.Trim();
        EnName = enName.Trim();
    }

    public void Update(string arName, string enName)
    {
        if (!IsActive)
            throw new DomainException("A deactivated address cannot be updated.");

        ArName = arName.Trim();
        EnName = enName.Trim();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The address is already deactivated.");

        IsActive = false;
    }
}