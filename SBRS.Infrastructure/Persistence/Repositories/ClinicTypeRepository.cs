using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class ClinicTypeRepository(AppDbContext db) : IClinicTypeRepository
{
    public Task<ClinicTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.ClinicTypes.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<ClinicTypeEntity>> GetAllAsync(CancellationToken ct) =>
        await db.ClinicTypes.AsNoTracking()
            .OrderBy(t => t.Code)
            .ToListAsync(ct);

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct) =>
        db.ClinicTypes.AnyAsync(t => t.Code == code && (excludeId == null || t.Id != excludeId), ct);

    public Task AddAsync(ClinicTypeEntity clinicType, CancellationToken ct)
    {
        db.ClinicTypes.Add(clinicType);
        return Task.CompletedTask;
    }
}
