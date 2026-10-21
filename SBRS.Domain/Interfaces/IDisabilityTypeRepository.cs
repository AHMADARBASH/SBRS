using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IDisabilityTypeRepository
{
    Task<DisabilityTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<DisabilityTypeEntity>> GetAllAsync(CancellationToken ct);
    Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct);
    Task AddAsync(DisabilityTypeEntity disabilityType, CancellationToken ct);
}
