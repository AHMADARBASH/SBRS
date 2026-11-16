using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class FacilityCategoryRepository(AppDbContext db) : IFacilityCategoryRepository
{
    public Task<FacilityCategoryEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.FacilityCategories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<FacilityCategoryEntity>> GetAllAsync(CancellationToken ct) =>
        await db.FacilityCategories.AsNoTracking()
            .OrderBy(c => c.EnName)
            .ToListAsync(ct);

    public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) =>
        db.FacilityCategories.AnyAsync(
            c => c.EnName == enName && (excludeId == null || c.Id != excludeId), ct);

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct) =>
        db.FacilityCategories.AnyAsync(
            c => c.Code == code && (excludeId == null || c.Id != excludeId), ct);

    public Task AddAsync(FacilityCategoryEntity category, CancellationToken ct)
    {
        db.FacilityCategories.Add(category);
        return Task.CompletedTask;
    }
}
