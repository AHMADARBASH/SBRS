using SBRS.Domain.Entities;

namespace SBRS.Domain.Interfaces;

public interface IEducationIssueRepository
{
    Task<EducationIssueEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<EducationIssueEntity>> GetAllAsync(CancellationToken ct);
}
