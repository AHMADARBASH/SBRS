using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Services;

public class BeneficiaryMatchService(IBeneficiaryRepository repository) : IBeneficiaryMatchService
{
    private const int MaxProbableMatches = 10;

    public async Task<BeneficiaryMatchSearchResult> FindAsync(BeneficiaryIdentityKey key,
        bool isBirthDateEstimated, Guid? excludeBeneficiaryId, CancellationToken cancellationToken,
        IReadOnlyCollection<(Guid IdentityTypeId, string Value)>? identities = null)
    {
        // Merged duplicates are not real persons any more; the excluded id is the record being reviewed.
        bool IsComparable(BeneficiaryEntity b) =>
            b.MatchReviewStatus != BeneficiaryMatchReviewStatus.MergedIntoExisting &&
            b.Id != excludeBeneficiaryId;

        // 1) Identity document matches first: the same type + document number is a 100% case,
        //    even when the five-field key differs (names may be misspelled or dates estimated).
        var seen = new HashSet<Guid>();
        var byDocument = (await repository.GetByExactIdentityAsync(
                identities ?? Array.Empty<(Guid, string)>(), excludeBeneficiaryId, cancellationToken))
            .Where(b => IsComparable(b) && seen.Add(b.Id))
            .Select(BeneficiaryMatchScorer.EvaluateSameIdentity)
            .ToList();

        // 2) Exact key matches: all five normalized fields equal.
        var sameKey = (await repository.GetByMatchKeyAsync(key.MatchKey, cancellationToken))
            .Where(b => IsComparable(b) && seen.Add(b.Id))
            .Select(b => BeneficiaryMatchScorer.Evaluate(key, isBirthDateEstimated, b))
            .ToList();

        if (byDocument.Count > 0 || sameKey.Count > 0)
        {
            var matches = byDocument.Concat(sameKey).ToList();
            return new BeneficiaryMatchSearchResult(BeneficiaryMatchStatus.Exact, matches);
        }

        // 3) Probable candidates: scored, threshold filtered, best first.
        var candidates = await repository.GetProbableCandidatesAsync(key, cancellationToken);
        var probable = candidates
            .Where(b => IsComparable(b))
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
