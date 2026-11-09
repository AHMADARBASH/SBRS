using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IChronicDiseaseRepository
{
    Task<ChronicDiseaseEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ChronicDiseaseEntity>> GetAllAsync(CancellationToken ct);
}
