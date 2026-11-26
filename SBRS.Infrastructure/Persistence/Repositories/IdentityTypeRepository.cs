using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class IdentityTypeRepository(AppDbContext db) : IIdentityTypeRepository
{
    public Task<IdentityTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.IdentityTypes.FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<IdentityTypeEntity>> GetByIdsAsync(IReadOnlyCollection<Guid> ids,
        CancellationToken ct)
    {
        if (ids.Count == 0) return [];

        var idList = ids.Distinct().ToList();
        return await db.IdentityTypes.AsNoTracking()
            .Where(i => idList.Contains(i.Id))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<IdentityTypeEntity>> GetAllAsync(CancellationToken ct) =>
        // index 0 means "not ordered yet" (legacy rows), so seeded explicit indexes come first.
        await db.IdentityTypes.AsNoTracking()
            .OrderBy(i => i.DisplayIndex == 0 ? 1 : 0)
            .ThenBy(i => i.DisplayIndex)
            .ThenBy(i => i.Code)
            .ToListAsync(ct);

    public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) =>
        db.IdentityTypes.AnyAsync(i => i.EnName == enName && (excludeId == null || i.Id != excludeId), ct);

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct) =>
        db.IdentityTypes.AnyAsync(i => i.Code == code && (excludeId == null || i.Id != excludeId), ct);

    public Task AddAsync(IdentityTypeEntity identityType, CancellationToken ct)
    {
        db.IdentityTypes.Add(identityType);
        return Task.CompletedTask;
    }
}
