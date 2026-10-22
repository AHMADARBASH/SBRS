using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IIdentityTypeRepository
{
    Task<IdentityTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<IdentityTypeEntity>> GetAllAsync(CancellationToken ct);
    Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct);
    Task AddAsync(IdentityTypeEntity identityType, CancellationToken ct);
}
