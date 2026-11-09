using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IMaritalStatusRepository
{
    Task<MaritalStatusEntity?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Stable-code lookup (the family rules key on MARRIED); rules never use generated ids.</summary>
    Task<MaritalStatusEntity?> GetByCodeAsync(string code, CancellationToken ct);

    Task<IReadOnlyList<MaritalStatusEntity>> GetAllAsync(CancellationToken ct);
}
