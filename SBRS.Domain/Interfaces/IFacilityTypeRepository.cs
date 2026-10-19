using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IFacilityTypeRepository
{
    Task<FacilityTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<FacilityTypeEntity>> GetAllAsync( CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct);
    Task AddAsync(FacilityTypeEntity type, CancellationToken ct);
}