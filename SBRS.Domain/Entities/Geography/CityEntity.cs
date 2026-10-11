using SBRS.Domain.Common;

public class City : LookupEntity
{
    public Guid CountryId { get; private set; }

    private City() { }
    public City(Guid countryId, string arName, string enName, string code)
        : base(arName, enName, code) => CountryId = countryId;
}