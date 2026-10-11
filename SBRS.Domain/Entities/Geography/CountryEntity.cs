using SBRS.Domain.Common;

public class Country : LookupEntity
{
    private Country() { }
    public Country(string arName, string enName, string code) : base(arName, enName, code) { }
}