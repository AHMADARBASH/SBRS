using Microsoft.EntityFrameworkCore;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class OrganizationRepository(AppDbContext db, ICurrentUser currentUser) : IOrganizationRepository
{
    // everything the current user is allowed to see
    private IQueryable<OrganizationEntity> Scoped()
    {
        if (currentUser.IsSystemAdmin)
            return db.Organizations;

        var organizationId = currentUser.OrganizationId;
        if (organizationId is null)
            return db.Organizations.Where(_ => false);   // no scope = nothing

        return db.Organizations.Where(o => o.Id == organizationId);
    }

    public Task<OrganizationEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Scoped().FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<List<OrganizationEntity>> GetAllAsync(bool includeInactive, CancellationToken ct = default)
        => Scoped().AsNoTracking()
                   .Where(o => includeInactive || o.IsActive)
                   .OrderBy(o => o.EnName)
                   .ToListAsync(ct);

    // NOT scoped on purpose: codes must be unique across the whole system
    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct = default)
        => db.Organizations.AnyAsync(
               o => o.Code == code && (excludeId == null || o.Id != excludeId), ct);

    public async Task AddAsync(OrganizationEntity organization, CancellationToken ct = default)
        => await db.Organizations.AddAsync(organization, ct);
}