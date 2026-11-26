using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IDiseaseCategoryRepository
{
    Task<DiseaseCategoryEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<DiseaseCategoryEntity>> GetAllAsync(CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct);
    Task AddAsync(DiseaseCategoryEntity category, CancellationToken ct);
}
