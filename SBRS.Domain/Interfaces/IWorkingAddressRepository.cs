using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IWorkingAddressRepository
{
    /// <summary>Scoped active row, tracked for update/delete; anything else looks missing (404).</summary>
    Task<WorkingAddressEntity?> GetByIdTrackedAsync(Guid id, Guid facilityId, CancellationToken ct);

    /// <summary>Scoped active row projected with the full geography chain in one query.</summary>
    Task<WorkingAddressRow?> GetByIdAsync(Guid id, Guid facilityId, CancellationToken ct);

    Task<(IReadOnlyList<WorkingAddressRow> Items, int TotalCount)> GetPagedAsync(Guid facilityId,
        int page, int pageSize, CancellationToken ct);

    Task<bool> DuplicateActiveAsync(Guid facilityId, Guid addressId, DateOnly workingDate,
        Guid? excludeId, CancellationToken ct);

    Task AddAsync(WorkingAddressEntity workingAddress, CancellationToken ct);
}
