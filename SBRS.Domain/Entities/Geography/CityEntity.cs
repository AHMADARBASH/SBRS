using SBRS.Domain.Common;

public class CityEntity : LookupEntity
{
    public Guid CountryId { get; private set; }

    private CityEntity() { }
    public CityEntity(Guid countryId, string arName, string enName, string code)
        : base(arName, enName, code) => CountryId = countryId;
}