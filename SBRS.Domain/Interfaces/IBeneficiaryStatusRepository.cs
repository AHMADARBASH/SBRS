using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IBeneficiaryStatusRepository
{
    Task<BeneficiaryStatusEntity?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Stable-code lookup (e.g. the NOT_AVAILABLE default); rules never use generated ids.</summary>
    Task<BeneficiaryStatusEntity?> GetByCodeAsync(string code, CancellationToken ct);

    Task<IReadOnlyList<BeneficiaryStatusEntity>> GetAllAsync(CancellationToken ct);
}
