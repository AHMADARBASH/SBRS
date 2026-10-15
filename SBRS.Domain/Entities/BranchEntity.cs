using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;


namespace SBRS.Domain.Entities;

public class  BranchEntity : BaseEntity
{
    public Guid organizationId { get; set; }
    public  string ArName { get; private set; } = default!;
    public  string EnName { get; private set; } = default!;
    public bool IsActive { get; set; } = true; 

    private BranchEntity() { } // EF Core 

    public BranchEntity(Guid organizationId, string arName, string enName,bool isActive)
    {
        this.organizationId = organizationId;
        ArName = arName.Trim();
        EnName = enName.Trim();
        IsActive = isActive;
    }

    public void update(string arName, string enName)
    {
        if (!IsActive)
            throw new DomainException("A deactivated branch cannot be updated.");
        ArName = arName.Trim();
        EnName = enName.Trim();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The branch is already deactivated.");

        IsActive = false;
    }
}
