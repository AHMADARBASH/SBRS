using SBRS.Application.Features.AddressTypes.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.AddressTypes.Mappings;

public static class AddressTypeMappings
{
    public static AddressTypeDto ToDto(this AddressTypeEntity e) =>
        new(e.Id, e.ArName, e.EnName);
}
