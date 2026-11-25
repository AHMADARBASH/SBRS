using Microsoft.EntityFrameworkCore;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Constants;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class FacilityRepository(AppDbContext db, ICurrentUser currentUser) : IFacilityRepository
{
    // everything the current user is allowed to see
    private IQueryable<FacilityEntity> Scoped()
    {
        if (currentUser.IsSystemAdmin)
            return db.Facilities;

        switch (currentUser.Role)
        {
            case Roles.OrganizationAdmin when currentUser.OrganizationId is { } organizationId:
                return db.Facilities.Where(f => f.Branch.organizationId == organizationId);

            case Roles.FacilityAdmin or Roles.DataEntry when currentUser.FacilityId is { } facilityId:
                return db.Facilities.Where(f => f.Id == facilityId);

            case Roles.BranchAdmin when currentUser.BranchId is { } branchId:
                return db.Facilities.Where(f => f.BranchId == branchId);

            // ProjectCoordinator is added when its scope data exists
            default:
                return db.Facilities.Where(_ => false);   // no scope = nothing
        }
    }

    public Task<FacilityEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Scoped().FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<FacilityEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => WithDetails(Scoped()).AsNoTracking()
                                .FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<List<FacilityEntity>> GetByBranchAsync(Guid branchId, bool includeInactive, CancellationToken ct = default)
        => WithDetails(Scoped()).AsNoTracking()
                   .Where(f => f.BranchId == branchId && (includeInactive || f.IsActive))
                   .OrderBy(f => f.EnName)
                   .ToListAsync(ct);

    // objects embedded in FacilityDto must be loaded explicitly: lazy loading is not enabled
    private static IQueryable<FacilityEntity> WithDetails(IQueryable<FacilityEntity> query)
        => query.Include(f => f.Branch)
                .Include(f => f.Area)
                .Include(f => f.FacilityType)
                .Include(f => f.Category);

    // NOT scoped on purpose: codes must be unique across the whole system
    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct = default)
        => db.Facilities.AnyAsync(
               f => f.Code == code && (excludeId == null || f.Id != excludeId), ct);

    // NOT scoped on purpose: a branch must not be deactivated while any facility under it is active
    public Task<bool> HasActiveFacilitiesAsync(Guid branchId, CancellationToken ct = default)
        => db.Facilities.AnyAsync(f => f.BranchId == branchId && f.IsActive, ct);

    /// <summary>
    /// Count used to generate the next sequence. NOT scoped and NOT filtered by IsActive:
    /// facilities are never hard-deleted (they are deactivated instead), so every facility
    /// ever created in the branch still owns its code and the sequence must keep growing.
    /// </summary>
    public Task<int> CountByBranchAsync(Guid branchId, CancellationToken ct = default)
        => db.Facilities.IgnoreQueryFilters().CountAsync(f => f.BranchId == branchId, ct);

    public void RemoveStaged(FacilityEntity facility)
        => db.Entry(facility).State = EntityState.Detached;

    public async Task AddAsync(FacilityEntity facility, CancellationToken ct = default)
        => await db.Facilities.AddAsync(facility, ct);
}