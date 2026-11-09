using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class MaritalStatusRepository(AppDbContext db) : IMaritalStatusRepository
{
    public Task<MaritalStatusEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.MaritalStatuses.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<MaritalStatusEntity?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.MaritalStatuses.FirstOrDefaultAsync(s => s.Code == code, ct);

    public async Task<IReadOnlyList<MaritalStatusEntity>> GetAllAsync(CancellationToken ct) =>
        await db.MaritalStatuses.AsNoTracking()
            .OrderBy(s => s.Code)
            .ToListAsync(ct);
}
