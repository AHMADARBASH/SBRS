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
}
