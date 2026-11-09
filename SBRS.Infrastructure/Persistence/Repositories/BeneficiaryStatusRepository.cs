using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class BeneficiaryStatusRepository(AppDbContext db) : IBeneficiaryStatusRepository
{
    public Task<BeneficiaryStatusEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.BeneficiaryStatuses.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<BeneficiaryStatusEntity?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.BeneficiaryStatuses.FirstOrDefaultAsync(s => s.Code == code, ct);

    public async Task<IReadOnlyList<BeneficiaryStatusEntity>> GetAllAsync(CancellationToken ct) =>
        await db.BeneficiaryStatuses.AsNoTracking()
            .OrderBy(s => s.Code)
            .ToListAsync(ct);
}
