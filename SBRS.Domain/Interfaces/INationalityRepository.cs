using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface INationalityRepository
{
    Task<NationalityEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<NationalityEntity>> GetAllAsync(CancellationToken ct);
    Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct);
    Task AddAsync(NationalityEntity nationality, CancellationToken ct);
}
