using SBRS.Application.Features.Organizations.DTOs;
using SBRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Features.Organizations.Mappings
{
    public static class OrganizationMappings
    {
        public static OrganizationDto ToDto(this OrganizationEntity e)
            => new(e.Id, e.ArName, e.EnName, e.Code, e.IsActive, e.CreatedAt, e.UpdatedAt);
    }
}
