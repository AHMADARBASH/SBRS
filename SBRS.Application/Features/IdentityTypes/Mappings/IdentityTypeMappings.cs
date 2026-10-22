using SBRS.Application.Features.IdentityTypes.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.IdentityTypes.Mappings;

public static class IdentityTypeMappings
{
    public static IdentityTypeDto ToDto(this IdentityTypeEntity i) =>
        new(i.Id, i.ArName, i.EnName, i.Code);
}
