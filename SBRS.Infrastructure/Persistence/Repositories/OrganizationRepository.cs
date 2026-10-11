using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class OrganizationRepository(AppDbContext db) : IOrganizationRepository
{
    public Task<OrganizationEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Organizations.FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<List<OrganizationEntity>> GetAllAsync(bool includeInactive, CancellationToken ct = default)
        => db.Organizations
             .AsNoTracking()
             .Where(o => includeInactive || o.IsActive)
             .OrderBy(o => o.EnName)
             .ToListAsync(ct);

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct = default)
        => db.Organizations.AnyAsync(
               o => o.Code == code && (excludeId == null || o.Id != excludeId), ct);

    public async Task AddAsync(OrganizationEntity organization, CancellationToken ct = default)
        => await db.Organizations.AddAsync(organization, ct);
}