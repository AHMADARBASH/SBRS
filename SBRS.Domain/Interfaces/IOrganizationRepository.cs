using SBRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Domain.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<IEnumerable<OrganizationEntity>> GetOrganizations();
        Task<OrganizationEntity> GetOrganizationById(Guid id);
        Task<OrganizationEntity> AddOrganization(OrganizationEntity organization);
        Task<OrganizationEntity> UpdateOrganization(OrganizationEntity organization);
        Task<bool> DeleteOrganization(Guid organizationId); 

    }
}
