using SBRS.Application.Features.MaritalStatuses.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.MaritalStatuses.Mappings;

public static class MaritalStatusMappings
{
    public static MaritalStatusDto ToDto(this MaritalStatusEntity status) =>
        new(status.Id, status.Code, status.ArName, status.EnName);
}
