using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IFacilityRepository
{
    Task<FacilityEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<FacilityEntity>> GetByBranchAsync(Guid branchId, bool includeInactive, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct = default);
    Task<bool> HasActiveFacilitiesAsync(Guid branchId, CancellationToken ct = default);
    Task AddAsync(FacilityEntity facility, CancellationToken ct = default);
}