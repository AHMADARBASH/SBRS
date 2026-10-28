using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IFacilityRepository
{
    Task<FacilityEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<FacilityEntity>> GetByBranchAsync(Guid branchId, bool includeInactive, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct = default);
    Task<bool> HasActiveFacilitiesAsync(Guid branchId, CancellationToken ct = default);

    /// <summary>
    /// How many facilities exist in the given city, including inactive ones
    /// (used to generate the next per-city sequence).
    /// </summary>
    Task<int> CountByCityAsync(Guid cityId, CancellationToken ct = default);

    /// <summary>Removes an unsaved (staged) row from the unit of work; used by the code-generation retry.</summary>
    void RemoveStaged(FacilityEntity facility);

    Task AddAsync(FacilityEntity facility, CancellationToken ct = default);
}
