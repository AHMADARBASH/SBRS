using SBRS.Application.Features.BeneficiaryStatuses.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.BeneficiaryStatuses.Mappings;

public static class BeneficiaryStatusMappings
{
    public static BeneficiaryStatusDto ToDto(this BeneficiaryStatusEntity status) =>
        new(status.Id, status.Code, status.ArName, status.EnName);
}
