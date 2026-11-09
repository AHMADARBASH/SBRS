using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class DisabilityReasonRepository(AppDbContext db) : IDisabilityReasonRepository
{
    public Task<DisabilityReasonEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.DisabilityReasons.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<DisabilityReasonEntity>> GetAllAsync(CancellationToken ct) =>
        await db.DisabilityReasons.AsNoTracking()
            .OrderBy(r => r.Code)
            .ToListAsync(ct);
}
