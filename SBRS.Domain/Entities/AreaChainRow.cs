using SBRS.Domain.Entities;

namespace SBRS.Domain.Entities;

/// <summary>One area with its parent chain, loaded by one query for a page of beneficiaries.</summary>
public sealed record AreaChainRow(
    Guid AreaId,
    AreaEntity Area,
    DistrictEntity? District,
    RegionEntity? Region,
    CityEntity? City,
    CountryEntity? Country);
