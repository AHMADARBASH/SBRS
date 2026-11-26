using SBRS.Application.Features.WorkingAddresses.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.WorkingAddresses.Mappings;

public static class WorkingAddressMappings
{
    public static WorkingAddressDto ToDto(this WorkingAddressRow r) =>
        new(r.Id, r.FacilityId, r.WorkingDate, r.IsActive,
            r.TypeId, r.TypeArName, r.TypeEnName,
            r.AddressId, r.AddressArName, r.AddressEnName,
            r.AreaId, r.AreaArName, r.AreaEnName,
            r.DistrictId, r.DistrictArName, r.DistrictEnName,
            r.RegionId, r.RegionArName, r.RegionEnName,
            r.CityId, r.CityArName, r.CityEnName,
            r.CountryId, r.CountryArName, r.CountryEnName,
            string.Join("، ", new[]
            {
                r.AddressArName, r.AreaArName, r.DistrictArName,
                r.RegionArName, r.CityArName, r.CountryArName
            }),
            string.Join(", ", new[]
            {
                r.AddressEnName, r.AreaEnName, r.DistrictEnName,
                r.RegionEnName, r.CityEnName, r.CountryEnName
            }));
}
