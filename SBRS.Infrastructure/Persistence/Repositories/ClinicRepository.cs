using Microsoft.EntityFrameworkCore;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Constants;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class ClinicRepository(AppDbContext db, ICurrentUser currentUser) : IClinicRepository
{
    private IQueryable<ClinicEntity> Scoped()
    {
        if (currentUser.IsSystemAdmin)
            return db.Clinics;

        return currentUser.Role switch
        {
            Roles.FacilityAdmin or Roles.DataEntry when currentUser.FacilityId is { } facilityId =>
                db.Clinics.Where(c => c.FacilityId == facilityId),
            _ => db.Clinics.Where(_ => false)
        };
    }

    public Task<ClinicEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Scoped().Include(c => c.ClinicType).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ClinicEntity>> GetAllAsync(Guid? facilityId, bool includeInactive,
        CancellationToken ct) =>
        await Scoped().AsNoTracking()
            .Include(c => c.ClinicType)
            .Where(c => facilityId == null || c.FacilityId == facilityId)
            .Where(c => includeInactive || c.IsActive)
            .OrderBy(c => c.ClinicType.ArName)
            .ToListAsync(ct);

    public Task<ClinicEntity?> GetByFacilityAndTypeAsync(Guid facilityId, Guid clinicTypeId,
        CancellationToken ct) =>
        db.Clinics.AsNoTracking()
            .FirstOrDefaultAsync(c => c.FacilityId == facilityId && c.ClinicTypeId == clinicTypeId, ct);

    public Task<bool> HasActiveDoctorLinksAsync(Guid clinicId, CancellationToken ct) =>
        db.ClinicDoctors.AnyAsync(l => l.ClinicId == clinicId && l.IsActive, ct);

    public async Task AddAsync(ClinicEntity clinic, CancellationToken ct) =>
        await db.Clinics.AddAsync(clinic, ct);
}
