using SBRS.Application.Features.DisabilityReasons.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.DisabilityReasons.Mappings;

public static class DisabilityReasonMappings
{
    public static DisabilityReasonDto ToDto(this DisabilityReasonEntity reason) =>
        new(reason.Id, reason.Code, reason.ArName, reason.EnName);
}
