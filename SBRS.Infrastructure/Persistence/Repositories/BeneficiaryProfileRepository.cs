using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence.Repositories;

public class BeneficiaryProfileRepository(AppDbContext db) : IBeneficiaryProfileRepository
{
    // tracked on purpose: the save handler needs to call Update on an existing profile
    public Task<BeneficiaryProfileEntity?> GetByBeneficiaryIdAsync(Guid beneficiaryId, CancellationToken ct) =>
        db.BeneficiaryProfiles.FirstOrDefaultAsync(x => x.BeneficiaryId == beneficiaryId, ct);

    public Task<IReadOnlyList<BeneficiaryDisabilityEntity>> GetDisabilitiesAsync(Guid beneficiaryId, CancellationToken ct) =>
        LoadAsync(db.BeneficiaryDisabilities.Where(x => x.BeneficiaryId == beneficiaryId)
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id), ct);

    public Task<IReadOnlyList<BeneficiaryChronicDiseaseEntity>> GetChronicDiseasesAsync(Guid beneficiaryId, CancellationToken ct) =>
        LoadAsync(db.BeneficiaryChronicDiseases.Where(x => x.BeneficiaryId == beneficiaryId)
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id), ct);

    public async Task AddAsync(BeneficiaryProfileEntity profile, CancellationToken ct) =>
        await db.BeneficiaryProfiles.AddAsync(profile, ct);

    public async Task AddAsync(BeneficiaryDisabilityEntity disability, CancellationToken ct) =>
        await db.BeneficiaryDisabilities.AddAsync(disability, ct);

    public async Task AddAsync(BeneficiaryChronicDiseaseEntity chronicDisease, CancellationToken ct) =>
        await db.BeneficiaryChronicDiseases.AddAsync(chronicDisease, ct);

    public void RemoveDisabilities(IEnumerable<BeneficiaryDisabilityEntity> disabilities) =>
        db.BeneficiaryDisabilities.RemoveRange(disabilities);

    public void RemoveChronicDiseases(IEnumerable<BeneficiaryChronicDiseaseEntity> chronicDiseases) =>
        db.BeneficiaryChronicDiseases.RemoveRange(chronicDiseases);

    private static async Task<IReadOnlyList<T>> LoadAsync<T>(IQueryable<T> query, CancellationToken ct)
        where T : notnull
        => await query.ToListAsync(ct);
}
