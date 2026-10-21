using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class IdentityTypeRepository(AppDbContext db) : IIdentityTypeRepository
{
    public Task<IdentityTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.IdentityTypes.FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<IdentityTypeEntity>> GetAllAsync(CancellationToken ct) =>
        await db.IdentityTypes.AsNoTracking()
            .OrderBy(i => i.EnName)
            .ToListAsync(ct);

    public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) =>
        db.IdentityTypes.AnyAsync(i => i.EnName == enName && (excludeId == null || i.Id != excludeId), ct);

    public Task AddAsync(IdentityTypeEntity identityType, CancellationToken ct)
    {
        db.IdentityTypes.Add(identityType);
        return Task.CompletedTask;
    }
}
