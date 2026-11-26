using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class AddressTypeRepository(AppDbContext db) : IAddressTypeRepository
{
    public Task<AddressTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.AddressTypes.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<AddressTypeEntity>> GetAllAsync(CancellationToken ct) =>
        await db.AddressTypes.AsNoTracking()
            .OrderBy(t => t.EnName)
            .ToListAsync(ct);

    public Task AddAsync(AddressTypeEntity type, CancellationToken ct)
    {
        db.AddressTypes.Add(type);
        return Task.CompletedTask;
    }
}
