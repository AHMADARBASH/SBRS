using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Features.Organizations.DTOs
{
    public record OrganizationDto(
        Guid Id,
        string ArName,
        string EnName,
        string Code,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
