using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;


namespace SBRS.Domain.Entities;

public class  BranchEntity : BaseEntity
{
    public Guid organizationId { get; set; }
    public string Code { get; private set; } = default!;
    public  string ArName { get; private set; } = default!;
    public  string EnName { get; private set; } = default!;
    public bool IsActive { get; set; } = true; 

    private BranchEntity() { } // EF Core 

    public BranchEntity(Guid organizationId, string arName, string enName, string code, bool isActive)
    {
        this.organizationId = organizationId;
        Set(arName, enName, code);
        IsActive = isActive;
    }

    public void update(string arName, string enName, string code)
    {
        if (!IsActive)
            throw new DomainException("A deactivated branch cannot be updated.");
        Set(arName, enName, code);
    }

    /// <summary>
    /// Applies the branch seed values to an existing row without reactivating it:
    /// seeding corrects the code and the Arabic name of a branch that is already there.
    /// </summary>
    public void ApplySeed(string arName, string code)
    {
        ArName = arName.Trim();
        Code = NormalizeCode(code);
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The branch is already deactivated.");

        IsActive = false;
    }

    private void Set(string arName, string enName, string code)
    {
        ArName = arName.Trim();
        EnName = enName.Trim();
        Code = NormalizeCode(code);
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Branch code is required.");

        var normalizedCode = code.Trim().ToUpperInvariant();
        if (normalizedCode.Length is < 2 or > 10 || !normalizedCode.All(char.IsLetter))
            throw new DomainException("Branch code must be 2 to 10 letters.");

        return normalizedCode;
    }
}
