using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class DiseaseCategoryRepository(AppDbContext db) : IDiseaseCategoryRepository
{
    public Task<DiseaseCategoryEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.DiseaseCategories.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<DiseaseCategoryEntity>> GetAllAsync(CancellationToken ct) =>
        await db.DiseaseCategories.AsNoTracking()
            .OrderBy(t => t.Code)
            .ToListAsync(ct);

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct) =>
        db.DiseaseCategories.AnyAsync(t => t.Code == code && (excludeId == null || t.Id != excludeId), ct);

    public Task AddAsync(DiseaseCategoryEntity category, CancellationToken ct)
    {
        db.DiseaseCategories.Add(category);
        return Task.CompletedTask;
    }
}
