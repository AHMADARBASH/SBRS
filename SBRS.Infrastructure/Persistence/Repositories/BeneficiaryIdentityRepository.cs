using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class BeneficiaryIdentityRepository(AppDbContext db) : IBeneficiaryIdentityRepository
{
    public async Task<IReadOnlyList<BeneficiaryIdentityEntity>> GetByBeneficiaryIdAsync(Guid beneficiaryId,
        CancellationToken ct) =>
        await db.BeneficiaryIdentities.AsNoTracking()
            .Include(x => x.IdentityType)
            .Where(x => x.BeneficiaryId == beneficiaryId)
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyDictionary<Guid, BeneficiaryIdentityEntity>> GetPrimaryByBeneficiaryIdsAsync(
        IReadOnlyCollection<Guid> beneficiaryIds, CancellationToken ct)
    {
        if (beneficiaryIds.Count == 0) return new Dictionary<Guid, BeneficiaryIdentityEntity>();

        var ids = beneficiaryIds.ToList();
        var rows = await db.BeneficiaryIdentities.AsNoTracking()
            .Include(x => x.IdentityType)
            .Where(x => ids.Contains(x.BeneficiaryId))
            .ToListAsync(ct);

        return rows
            .GroupBy(x => x.BeneficiaryId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).First());
    }

    public async Task AddAsync(BeneficiaryIdentityEntity identity, CancellationToken ct) =>
        await db.BeneficiaryIdentities.AddAsync(identity, ct);
}
