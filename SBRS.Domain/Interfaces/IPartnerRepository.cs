using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IPartnerRepository
{
    Task<PartnerEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<PartnerEntity>> GetAllAsync(CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct);
    Task AddAsync(PartnerEntity partner, CancellationToken ct);
}