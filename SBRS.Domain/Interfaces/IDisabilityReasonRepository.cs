using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IDisabilityReasonRepository
{
    Task<DisabilityReasonEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<DisabilityReasonEntity>> GetAllAsync(CancellationToken ct);
}
