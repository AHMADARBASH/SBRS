using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class FacilityTypeRepository(AppDbContext db) : IFacilityTypeRepository
{
    public Task<FacilityTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.FacilityTypes.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<FacilityTypeEntity>> GetAllAsync( CancellationToken ct) =>
        await db.FacilityTypes.AsNoTracking()
            .OrderBy(t => t.Code)
            .ToListAsync(ct);

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct) =>
        db.FacilityTypes.AnyAsync(t => t.Code == code && (excludeId == null || t.Id != excludeId), ct);


    public Task AddAsync(FacilityTypeEntity type, CancellationToken ct)
    {
        db.FacilityTypes.Add(type);
        return Task.CompletedTask;
    }
}