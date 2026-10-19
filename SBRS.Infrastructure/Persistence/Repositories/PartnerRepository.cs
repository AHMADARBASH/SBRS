using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class PartnerRepository(AppDbContext db) : IPartnerRepository
{
    public Task<PartnerEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Partners.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<PartnerEntity>> GetAllAsync(CancellationToken ct) =>
        await db.Partners.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct) =>
        db.Partners.AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId), ct);

    public Task AddAsync(PartnerEntity partner, CancellationToken ct)
    {
        db.Partners.Add(partner);
        return Task.CompletedTask;
    }
}