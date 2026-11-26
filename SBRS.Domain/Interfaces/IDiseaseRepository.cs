using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IDiseaseRepository
{
    Task<DiseaseEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<DiseaseEntity>> GetAllAsync(Guid? diseaseCategoryId, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct);
    Task AddAsync(DiseaseEntity disease, CancellationToken ct);
}
