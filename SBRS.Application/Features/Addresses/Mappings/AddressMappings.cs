using SBRS.Application.Features.Addresses.DTOs;
using SBRS.Domain.Entities;
using System.Net;

namespace SBRS.Application.Features.Addresses.Mappings;

public static class AddressMappings
{
    public static AddressDto ToDto(this AddressEntity e)
        => new(e.Id, e.AreaId, e.ArName, e.EnName, e.IsActive, e.CreatedAt, e.UpdatedAt);
}