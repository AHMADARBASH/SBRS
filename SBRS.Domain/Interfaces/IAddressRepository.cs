using SBRS.Domain.Entities;
using System.Net;

namespace SBRS.Domain.Interfaces;

public interface IAddressRepository
{
    Task<AddressEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<AddressEntity>> GetByAreaAsync(Guid areaId, bool includeInactive, CancellationToken ct = default);
    Task<bool> NameExistsAsync(Guid areaId, string arName, string enName, Guid? excludeId, CancellationToken ct = default);
    Task AddAsync(AddressEntity address, CancellationToken ct = default);
}