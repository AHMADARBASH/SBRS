using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;
using System.Net;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class AddressRepository(AppDbContext db) : IAddressRepository
{
    public Task<AddressEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Addresses.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<List<AddressEntity>> GetByAreaAsync(Guid areaId, bool includeInactive, CancellationToken ct = default)
        => db.Addresses.AsNoTracking()
             .Where(a => a.AreaId == areaId && (includeInactive || a.IsActive))
             .OrderBy(a => a.EnName)
             .ToListAsync(ct);

    public Task<bool> NameExistsAsync(
        Guid areaId, string arName, string enName, Guid? excludeId, CancellationToken ct = default)
        => db.Addresses.AnyAsync(
               a => a.AreaId == areaId
                    && (a.ArName == arName || a.EnName == enName)
                    && (excludeId == null || a.Id != excludeId), ct);

    public async Task AddAsync(AddressEntity address, CancellationToken ct = default)
        => await db.Addresses.AddAsync(address, ct);
}