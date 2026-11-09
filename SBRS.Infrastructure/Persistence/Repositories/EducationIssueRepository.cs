using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class EducationIssueRepository(AppDbContext db) : IEducationIssueRepository
{
    public Task<EducationIssueEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.EducationIssues.FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<EducationIssueEntity>> GetAllAsync(CancellationToken ct) =>
        await db.EducationIssues.AsNoTracking()
            .OrderBy(i => i.Code)
            .ToListAsync(ct);
}
