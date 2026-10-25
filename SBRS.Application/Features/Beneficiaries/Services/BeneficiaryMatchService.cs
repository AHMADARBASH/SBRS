using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Services;

public class BeneficiaryMatchService(IBeneficiaryRepository repository) : IBeneficiaryMatchService
{
    private const int MaxProbableMatches = 10;

    public async Task<BeneficiaryMatchSearchResult> FindAsync(BeneficiaryIdentityKey key,
        bool isBirthDateEstimated, Guid? excludeBeneficiaryId, CancellationToken cancellationToken)
    {
        // Merged duplicates are not real persons any more; the excluded id is the record being reviewed.
        bool IsComparable(BeneficiaryEntity b) =>
            b.MatchReviewStatus != BeneficiaryMatchReviewStatus.MergedIntoExisting &&
            b.Id != excludeBeneficiaryId;

        var sameKey = (await repository.GetByMatchKeyAsync(key.MatchKey, cancellationToken))
            .Where(IsComparable)
            .ToList();

        if (sameKey.Count > 0)
        {
            var exact = sameKey
                .Select(b => BeneficiaryMatchScorer.Evaluate(key, isBirthDateEstimated, b))
                .ToList();
            return new BeneficiaryMatchSearchResult(BeneficiaryMatchStatus.Exact, exact);
        }

        var candidates = await repository.GetProbableCandidatesAsync(key, cancellationToken);
        var probable = candidates
            .Where(IsComparable)
            .Select(b => BeneficiaryMatchScorer.Evaluate(key, isBirthDateEstimated, b))
            .Where(m => m.Status == BeneficiaryMatchStatus.Probable)
            .OrderByDescending(m => m.Score)
            .Take(MaxProbableMatches)
            .ToList();

        return probable.Count > 0
            ? new BeneficiaryMatchSearchResult(BeneficiaryMatchStatus.Probable, probable)
            : BeneficiaryMatchSearchResult.NotFound;
    }
}
