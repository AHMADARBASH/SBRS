using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class ClinicDoctorRepository(AppDbContext db) : IClinicDoctorRepository
{
    public Task<ClinicDoctorEntity?> GetAsync(Guid clinicId, Guid doctorId, CancellationToken ct) =>
        db.ClinicDoctors.FirstOrDefaultAsync(l => l.ClinicId == clinicId && l.DoctorId == doctorId, ct);

    public async Task<IReadOnlyList<ClinicDoctorEntity>> GetActiveByDoctorAsync(Guid doctorId,
        CancellationToken ct) =>
        await db.ClinicDoctors.Where(l => l.DoctorId == doctorId && l.IsActive).ToListAsync(ct);

    public async Task AddAsync(ClinicDoctorEntity link, CancellationToken ct) =>
        await db.ClinicDoctors.AddAsync(link, ct);
}
