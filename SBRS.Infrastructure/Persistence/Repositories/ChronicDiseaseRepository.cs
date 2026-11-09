using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class ChronicDiseaseRepository(AppDbContext db) : IChronicDiseaseRepository
{
    public Task<ChronicDiseaseEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.ChronicDiseases.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<ChronicDiseaseEntity>> GetAllAsync(CancellationToken ct) =>
        await db.ChronicDiseases.AsNoTracking()
            .OrderBy(d => d.Code)
            .ToListAsync(ct);
}
