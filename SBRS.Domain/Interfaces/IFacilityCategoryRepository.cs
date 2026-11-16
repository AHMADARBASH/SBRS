using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IFacilityCategoryRepository
{
    Task<FacilityCategoryEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<FacilityCategoryEntity>> GetAllAsync(CancellationToken ct);
    Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct);
    Task AddAsync(FacilityCategoryEntity category, CancellationToken ct);
}
