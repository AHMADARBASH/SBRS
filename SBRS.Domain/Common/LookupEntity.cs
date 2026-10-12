namespace SBRS.Domain.Common;

public abstract class LookupEntity : BaseEntity
{
    public string ArName { get; protected set; } = default!;
    public string EnName { get; protected set; } = default!;
    public string Code { get; protected set; } = default!;

    protected LookupEntity() { }   // EF Core

    protected LookupEntity(string arName, string enName, string code)
    {
        ArName = arName.Trim();
        EnName = enName.Trim();
        Code = code.Trim();
    }
}