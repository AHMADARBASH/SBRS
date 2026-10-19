using SBRS.Application.Features.FacilityPartners.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.FacilityPartners.Mappings;

public static class FacilityPartnerMappings
{
    // The Partner navigation must be loaded or set.
    public static FacilityPartnerDto ToDto(this FacilityPartnerEntity e) => new(
        e.Id, e.FacilityId, e.PartnerId, e.Partner.ArName, e.Partner.EnName,
        e.ProjectCode, e.StartDate, e.EndDate, e.IsCurrent);
}