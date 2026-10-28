using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

public class CityEntity : LookupEntity
{
    public Guid CountryId { get; private set; }

    /// <summary>Three-letter city code used to build facility codes, e.g. DAM.</summary>
    public string C_Code { get; private set; } = default!;

    private CityEntity() { }   // EF Core
    public CityEntity(Guid countryId, string arName, string enName, string code, string cCode)
        : base(arName, enName, code)
    {
        if (countryId == Guid.Empty) throw new DomainException("Country is required.");
        if (string.IsNullOrWhiteSpace(cCode) || cCode.Trim().Length != 3)
            throw new DomainException("The city c_code must be exactly 3 characters.");

        CountryId = countryId;
        C_Code = cCode.Trim().ToUpperInvariant();
    }
}
