using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IBeneficiaryConsultationRepository
{
    /// <summary>Active consultation of the user's facility; anything else looks missing (404).</summary>
    Task<BeneficiaryConsultationEntity?> GetByIdAsync(Guid id, Guid facilityId, CancellationToken ct);

    Task<(IReadOnlyList<BeneficiaryConsultationEntity> Items, int TotalCount)> GetPagedAsync(Guid facilityId,
        Guid? visitId, Guid? clinicId, Guid? doctorId, int page, int pageSize, CancellationToken ct);

    Task<IReadOnlyList<BeneficiaryConsultationEntity>> GetByVisitIdAsync(Guid visitId, Guid facilityId,
        CancellationToken ct);

    /// <summary>Tracked active rows, deactivated together with their visit.</summary>
    Task<IReadOnlyList<BeneficiaryConsultationEntity>> GetActiveByVisitIdAsync(Guid visitId,
        CancellationToken ct);

    Task<bool> ExistsAsync(Guid visitId, Guid clinicId, Guid doctorId, Guid? excludeId, CancellationToken ct);

    Task AddAsync(BeneficiaryConsultationEntity consultation, CancellationToken ct);

    /// <summary>Tracked links (active and inactive) of one consultation.</summary>
    Task<IReadOnlyList<BeneficiaryConsultationDetailEntity>> GetDetailsByConsultationIdAsync(
        Guid consultationId, CancellationToken ct);

    /// <summary>Tracked active links of all consultations of a visit (used by the visit soft delete).</summary>
    Task<IReadOnlyList<BeneficiaryConsultationDetailEntity>> GetActiveDetailsByVisitIdAsync(
        Guid visitId, CancellationToken ct);

    Task AddDetailAsync(BeneficiaryConsultationDetailEntity detail, CancellationToken ct);
}
