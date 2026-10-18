using SBRS.Application.Features.Facilities.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.Facilities.Mappings;

public static class FacilityMappings
{
    public static FacilityDto ToDto(this FacilityEntity e)
     => new(e.Id, e.BranchId, e.AddressId, e.ArName, e.EnName, e.Code, e.IsActive, e.CreatedAt, e.UpdatedAt);
}