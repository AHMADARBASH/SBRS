using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class IdentityTypeEntity : BaseEntity
{
    private IdentityTypeEntity() { }   // EF

    public IdentityTypeEntity(string arName, string enName, string code)
    {
        Set(arName, enName, code);
    }

    public string ArName { get; private set; } = default!;
    public string EnName { get; private set; } = default!;
    public string Code { get; private set; } = default!;

    public void Update(string arName, string enName, string code) => Set(arName, enName, code);

    private void Set(string arName, string enName, string code)
    {
        if (string.IsNullOrWhiteSpace(arName) || string.IsNullOrWhiteSpace(enName) || string.IsNullOrWhiteSpace(code))
            throw new DomainException("Arabic name, English name and code are required.");

        ArName = arName.Trim();
        EnName = enName.Trim();
        Code = code.Trim();
    }
}
