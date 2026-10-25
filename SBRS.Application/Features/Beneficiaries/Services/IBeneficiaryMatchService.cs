using SBRS.Domain.Common;
using SBRS.Domain.Enums;

namespace SBRS.Application.Features.Beneficiaries.Services;

public interface IBeneficiaryMatchService
{
    /// <summary>
    /// Searches across all facilities. Returns Exact when the MatchKey exists, Probable when
    /// scored candidates reach the threshold (best first), otherwise NotFound.
    /// Merged duplicates and the excluded beneficiary are ignored.
    /// </summary>
    Task<BeneficiaryMatchSearchResult> FindAsync(BeneficiaryIdentityKey key, bool isBirthDateEstimated,
        Guid? excludeBeneficiaryId, CancellationToken cancellationToken);
}

public sealed record BeneficiaryMatchSearchResult(
    BeneficiaryMatchStatus Status,
    IReadOnlyList<BeneficiaryMatch> Matches)
{
    public static BeneficiaryMatchSearchResult NotFound { get; } =
        new(BeneficiaryMatchStatus.NotFound, Array.Empty<BeneficiaryMatch>());
}
