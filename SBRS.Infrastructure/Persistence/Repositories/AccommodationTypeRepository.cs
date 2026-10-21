using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class AccommodationTypeRepository(AppDbContext db) : IAccommodationTypeRepository
{
    public Task<AccommodationTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.AccommodationTypes.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<AccommodationTypeEntity>> GetAllAsync(CancellationToken ct) =>
        await db.AccommodationTypes.AsNoTracking()
            .OrderBy(a => a.EnName)
            .ToListAsync(ct);

    public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) =>
        db.AccommodationTypes.AnyAsync(a => a.EnName == enName && (excludeId == null || a.Id != excludeId), ct);

    public Task AddAsync(AccommodationTypeEntity accommodationType, CancellationToken ct)
    {
        db.AccommodationTypes.Add(accommodationType);
        return Task.CompletedTask;
    }
}
