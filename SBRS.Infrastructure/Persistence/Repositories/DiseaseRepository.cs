using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class DiseaseRepository(AppDbContext db) : IDiseaseRepository
{
    public Task<DiseaseEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Diseases.Include(d => d.DiseaseCategory).FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<DiseaseEntity>> GetAllAsync(Guid? diseaseCategoryId, CancellationToken ct) =>
        await db.Diseases.AsNoTracking()
            .Include(d => d.DiseaseCategory)
            .Where(d => diseaseCategoryId == null || d.DiseaseCategoryId == diseaseCategoryId)
            .OrderBy(d => d.Code)
            .ToListAsync(ct);

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct) =>
        db.Diseases.AnyAsync(d => d.Code == code && (excludeId == null || d.Id != excludeId), ct);

    public Task AddAsync(DiseaseEntity disease, CancellationToken ct)
    {
        db.Diseases.Add(disease);
        return Task.CompletedTask;
    }
}
