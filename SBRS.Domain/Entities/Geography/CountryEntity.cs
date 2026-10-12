using SBRS.Domain.Common;

public class CountryEntity : LookupEntity
{
    private CountryEntity() { }
    public CountryEntity(string arName, string enName, string code) : base(arName, enName, code) { }
}