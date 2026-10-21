using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class DisabilityTypeRepository(AppDbContext db) : IDisabilityTypeRepository
{
    public Task<DisabilityTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.DisabilityTypes.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<DisabilityTypeEntity>> GetAllAsync(CancellationToken ct) =>
        await db.DisabilityTypes.AsNoTracking()
            .OrderBy(d => d.EnName)
            .ToListAsync(ct);

    public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) =>
        db.DisabilityTypes.AnyAsync(d => d.EnName == enName && (excludeId == null || d.Id != excludeId), ct);

    public Task AddAsync(DisabilityTypeEntity disabilityType, CancellationToken ct)
    {
        db.DisabilityTypes.Add(disabilityType);
        return Task.CompletedTask;
    }
}
