using SBRS.Application.Features.Partners.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.Partners.Mappings;

public static class PartnerMappings
{
    public static PartnerDto ToDto(this PartnerEntity p) => new(p.Id, p.ArName, p.EnName, p.Code);
}