using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;
using SBRS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Infrastructure.Repositories
{
    internal class OrganizationRepository(SBRSDbContext dbContext) : IOrganizationRepository
    {
        

        public async Task<IEnumerable<OrganizationEntity>> GetOrganizations()
        {
            return await dbContext.Organizations.ToListAsync();
        }

        public async Task<OrganizationEntity> GetOrganizationById(Guid id)
        {
            return await dbContext.Organizations.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OrganizationEntity> AddOrganization(OrganizationEntity organization)
        {
            await dbContext.Organizations.AddAsync(organization);
            await dbContext.SaveChangesAsync();
            return organization;
        }

        public async Task<OrganizationEntity> UpdateOrganization(OrganizationEntity organization)
        {
            var existingOrganization = dbContext.Organizations.FirstOrDefault(o => o.Id == organization.Id);
            if(existingOrganization != null)
            {
                existingOrganization.ArName = organization.ArName;
                existingOrganization.EnName = organization.EnName;
                await dbContext.SaveChangesAsync();
                return existingOrganization;
            }
            return organization;
        }

        public async Task<bool> DeleteOrganization(Guid organizationId)
        {
            var existingOrganization = dbContext.Organizations.FirstOrDefault(o => o.Id == organizationId);
            if (existingOrganization != null)
            {
                dbContext.Organizations.Remove(existingOrganization);
                return await dbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
