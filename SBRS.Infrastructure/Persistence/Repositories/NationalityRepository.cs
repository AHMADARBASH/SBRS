using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class NationalityRepository(AppDbContext db) : INationalityRepository
{
    public Task<NationalityEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Nationalities.FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<IReadOnlyList<NationalityEntity>> GetAllAsync(CancellationToken ct) =>
        await db.Nationalities.AsNoTracking()
            .OrderBy(n => n.EnName)
            .ToListAsync(ct);

    public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) =>
        db.Nationalities.AnyAsync(n => n.EnName == enName && (excludeId == null || n.Id != excludeId), ct);

    public Task AddAsync(NationalityEntity nationality, CancellationToken ct)
    {
        db.Nationalities.Add(nationality);
        return Task.CompletedTask;
    }
}
