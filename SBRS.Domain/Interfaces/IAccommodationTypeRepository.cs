using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IAccommodationTypeRepository
{
    Task<AccommodationTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<AccommodationTypeEntity>> GetAllAsync(CancellationToken ct);
    Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct);
    Task AddAsync(AccommodationTypeEntity accommodationType, CancellationToken ct);
}
