using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class EducationTypeRepository(AppDbContext db) : IEducationTypeRepository
{
    public Task<EducationTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.EducationTypes.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<EducationTypeEntity>> GetAllAsync(CancellationToken ct) =>
        await db.EducationTypes.AsNoTracking()
            .OrderBy(e => e.EnName)
            .ToListAsync(ct);

    public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) =>
        db.EducationTypes.AnyAsync(e => e.EnName == enName && (excludeId == null || e.Id != excludeId), ct);

    public Task AddAsync(EducationTypeEntity educationType, CancellationToken ct)
    {
        db.EducationTypes.Add(educationType);
        return Task.CompletedTask;
    }
}
