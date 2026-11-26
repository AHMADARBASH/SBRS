using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IClinicTypeRepository
{
    Task<ClinicTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ClinicTypeEntity>> GetAllAsync(CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct);
    Task AddAsync(ClinicTypeEntity clinicType, CancellationToken ct);
}
