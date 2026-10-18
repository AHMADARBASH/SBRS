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

            // BranchAdmin and ProjectCoordinator are added when their scope data exists
            default:
                return db.Facilities.Where(_ => false);   // no scope = nothing
        }
    }

    public Task<FacilityEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Scoped().FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<List<FacilityEntity>> GetByBranchAsync(Guid branchId, bool includeInactive, CancellationToken ct = default)
        => Scoped().AsNoTracking()
                   .Where(f => f.BranchId == branchId && (includeInactive || f.IsActive))
                   .OrderBy(f => f.EnName)
                   .ToListAsync(ct);

    // NOT scoped on purpose: codes must be unique across the whole system
    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct = default)
        => db.Facilities.AnyAsync(
               f => f.Code == code && (excludeId == null || f.Id != excludeId), ct);

    // NOT scoped on purpose: a branch must not be deactivated while any facility under it is active
    public Task<bool> HasActiveFacilitiesAsync(Guid branchId, CancellationToken ct = default)
        => db.Facilities.AnyAsync(f => f.BranchId == branchId && f.IsActive, ct);

    public async Task AddAsync(FacilityEntity facility, CancellationToken ct = default)
        => await db.Facilities.AddAsync(facility, ct);
}