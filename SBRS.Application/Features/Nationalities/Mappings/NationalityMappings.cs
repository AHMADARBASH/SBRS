using SBRS.Application.Features.Nationalities.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.Nationalities.Mappings;

public static class NationalityMappings
{
    public static NationalityDto ToDto(this NationalityEntity n) =>
        new(n.Id, n.ArName, n.EnName);
}
