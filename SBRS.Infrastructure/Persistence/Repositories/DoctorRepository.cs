using Microsoft.EntityFrameworkCore;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Constants;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class DoctorRepository(AppDbContext db, ICurrentUser currentUser) : IDoctorRepository
{
    private IQueryable<DoctorEntity> Scoped()
    {
        if (currentUser.IsSystemAdmin)
            return db.Doctors;

        return currentUser.Role switch
        {
            Roles.FacilityAdmin or Roles.DataEntry when currentUser.FacilityId is { } facilityId =>
                db.Doctors.Where(d => d.FacilityId == facilityId),
            _ => db.Doctors.Where(_ => false)
        };
    }

    private static IQueryable<DoctorEntity> WithClinics(IQueryable<DoctorEntity> query) =>
        query.Include(d => d.ClinicDoctors.Where(l => l.IsActive))
             .ThenInclude(l => l.Clinic)
             .ThenInclude(c => c.ClinicType);

    public Task<DoctorEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Scoped().FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<DoctorEntity?> GetByIdWithClinicsAsync(Guid id, CancellationToken ct) =>
        WithClinics(Scoped()).AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<DoctorEntity>> GetAllAsync(Guid? facilityId, Guid? clinicId,
        bool includeInactive, CancellationToken ct) =>
        await WithClinics(Scoped()).AsNoTracking()
            .Where(d => facilityId == null || d.FacilityId == facilityId)
            .Where(d => clinicId == null || db.ClinicDoctors.Any(
                l => l.DoctorId == d.Id && l.IsActive && l.ClinicId == clinicId))
            .Where(d => includeInactive || d.IsActive)
            .OrderBy(d => d.LastName).ThenBy(d => d.FirstName)
            .ToListAsync(ct);

    public async Task AddAsync(DoctorEntity doctor, CancellationToken ct) =>
        await db.Doctors.AddAsync(doctor, ct);
}
