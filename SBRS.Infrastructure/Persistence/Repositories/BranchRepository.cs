using Microsoft.EntityFrameworkCore;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Infrastructure.Persistence.Repositories
{
    public class BranchRepository(AppDbContext db, ICurrentUser currentUser) : IBranchRepository
    {

        private IQueryable<BranchEntity> Scoped()
        {
            if (currentUser.IsSystemAdmin)
                return db.Branches;

            var organizationId = currentUser.OrganizationId;
            if (organizationId is null)
                return db.Branches.Where(_ => false);   // no scope = nothing

            return db.Branches.Where(b => b.organizationId == organizationId);
        }

        public async Task<List<BranchEntity>> GetAllAsync(CancellationToken ct = default)
        {
            return await Scoped().AsNoTracking().ToListAsync(ct);
        }

        public async Task<BranchEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var branch = Scoped().Where(b => b.Id == id);
            return await branch.FirstOrDefaultAsync(ct);
        }

        public async Task<List<BranchEntity>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken ct = default)
        {
            var branch = Scoped().Where(b => b.organizationId == organizationId);
            return await branch.ToListAsync(ct);
        }

        public async Task AddAsync(BranchEntity branch, CancellationToken ct = default)
        {
            await db.Branches.AddAsync(branch,ct);
        }


       
    }
}
