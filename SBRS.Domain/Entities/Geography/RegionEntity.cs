using SBRS.Domain.Common;


public class RegionEntity : LookupEntity
{
    public Guid CityId { get; private set; }

    private RegionEntity() { }
    public RegionEntity(Guid cityId, string arName, string enName, string code)
        : base(arName, enName, code) => CityId = cityId;
}