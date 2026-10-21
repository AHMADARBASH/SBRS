using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IEducationTypeRepository
{
    Task<EducationTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<EducationTypeEntity>> GetAllAsync(CancellationToken ct);
    Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct);
    Task AddAsync(EducationTypeEntity educationType, CancellationToken ct);
}
