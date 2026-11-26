using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class BeneficiaryConsultationRepository(AppDbContext db) : IBeneficiaryConsultationRepository
{
    private static IQueryable<BeneficiaryConsultationEntity> WithDetails(
        IQueryable<BeneficiaryConsultationEntity> query) =>
        query.Include(c => c.Clinic).ThenInclude(cl => cl.ClinicType)
             .Include(c => c.Doctor)
             .Include(c => c.ConsultationDetails.Where(d => d.IsActive))
                .ThenInclude(d => d.Disease).ThenInclude(d => d.DiseaseCategory);

    private IQueryable<BeneficiaryConsultationEntity> Scoped(Guid facilityId) =>
        db.BeneficiaryConsultations.Where(c =>
            db.Visits.Any(v => v.Id == c.VisitId && v.FacilityId == facilityId));

    public Task<BeneficiaryConsultationEntity?> GetByIdAsync(Guid id, Guid facilityId, CancellationToken ct) =>
        WithDetails(Scoped(facilityId))
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, ct);

    public async Task<(IReadOnlyList<BeneficiaryConsultationEntity> Items, int TotalCount)> GetPagedAsync(
        Guid facilityId, Guid? visitId, Guid? clinicId, Guid? doctorId,
        int page, int pageSize, CancellationToken ct)
    {
        var query = WithDetails(Scoped(facilityId).AsNoTracking())
            .Where(c => c.IsActive)
            .Where(c => visitId == null || c.VisitId == visitId)
            .Where(c => clinicId == null || c.ClinicId == clinicId)
            .Where(c => doctorId == null || c.DoctorId == doctorId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.ConsultationDate)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<BeneficiaryConsultationEntity>> GetByVisitIdAsync(Guid visitId,
        Guid facilityId, CancellationToken ct) =>
        await WithDetails(Scoped(facilityId).AsNoTracking())
            .Where(c => c.VisitId == visitId && c.IsActive)
            .OrderBy(c => c.ConsultationDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BeneficiaryConsultationEntity>> GetActiveByVisitIdAsync(Guid visitId,
        CancellationToken ct) =>
        await db.BeneficiaryConsultations
            .Where(c => c.VisitId == visitId && c.IsActive)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(Guid visitId, Guid clinicId, Guid doctorId, Guid? excludeId,
        CancellationToken ct) =>
        db.BeneficiaryConsultations.AnyAsync(c =>
            c.IsActive && c.VisitId == visitId && c.ClinicId == clinicId && c.DoctorId == doctorId &&
            (excludeId == null || c.Id != excludeId), ct);

    public async Task AddAsync(BeneficiaryConsultationEntity consultation, CancellationToken ct) =>
        await db.BeneficiaryConsultations.AddAsync(consultation, ct);

    public async Task<IReadOnlyList<BeneficiaryConsultationDetailEntity>> GetDetailsByConsultationIdAsync(
        Guid consultationId, CancellationToken ct) =>
        await db.BeneficiaryConsultationDetails
            .Where(d => d.BeneficiaryConsultationId == consultationId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BeneficiaryConsultationDetailEntity>> GetActiveDetailsByVisitIdAsync(
        Guid visitId, CancellationToken ct) =>
        await db.BeneficiaryConsultationDetails
            .Where(d => d.IsActive && db.BeneficiaryConsultations.Any(
                c => c.Id == d.BeneficiaryConsultationId && c.VisitId == visitId))
            .ToListAsync(ct);

    public async Task AddDetailAsync(BeneficiaryConsultationDetailEntity detail, CancellationToken ct) =>
        await db.BeneficiaryConsultationDetails.AddAsync(detail, ct);
}
