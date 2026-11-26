using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IVisitRepository
{
    /// <summary>Active visit of the given facility; any other visit looks missing (404).</summary>
    Task<VisitEntity?> GetByIdAsync(Guid id, Guid facilityId, CancellationToken ct);

    Task<(IReadOnlyList<VisitEntity> Items, int TotalCount)> GetPagedAsync(Guid facilityId,
        Guid? beneficiaryId, int page, int pageSize, CancellationToken ct);

    Task AddAsync(VisitEntity visit, CancellationToken ct);
}
