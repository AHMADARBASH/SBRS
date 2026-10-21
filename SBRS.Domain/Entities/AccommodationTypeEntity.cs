using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class AccommodationTypeEntity : BaseEntity
{
    private AccommodationTypeEntity() { }   // EF

    public AccommodationTypeEntity(string arName, string enName)
    {
        Set(arName, enName);
    }

    public string ArName { get; private set; } = default!;
    public string EnName { get; private set; } = default!;

    public void Update(string arName, string enName) => Set(arName, enName);

    private void Set(string arName, string enName)
    {
        if (string.IsNullOrWhiteSpace(arName) || string.IsNullOrWhiteSpace(enName))
            throw new DomainException("Arabic name and English name are required.");

        ArName = arName.Trim();
        EnName = enName.Trim();
    }
}
