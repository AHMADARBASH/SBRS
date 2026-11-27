using SBRS.Application.Features.Geography.DTOs;

namespace SBRS.Application.Features.Beneficiaries.DTOs;

/// <summary>The area of a beneficiary with its full parent chain.</summary>
public sealed record BeneficiaryAreaDto(
    AreaDto Area,
    DistrictDto? District,
    RegionDto? Region,
    CityDto? City,
    CountryDto? Country);
