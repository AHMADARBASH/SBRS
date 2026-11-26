using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IAddressTypeRepository
{
    Task<AddressTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<AddressTypeEntity>> GetAllAsync(CancellationToken ct);
    Task AddAsync(AddressTypeEntity type, CancellationToken ct);
}
