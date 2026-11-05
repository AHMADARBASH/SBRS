using SBRS.Domain.Common;
using SBRS.Domain.Enums;

namespace SBRS.Application.Features.Beneficiaries.Services;

public interface IBeneficiaryMatchService
{
    /// <summary>
    /// Searches across all facilities. Returns Exact when the MatchKey exists, an identity
    /// document (type + number) matches another beneficiary, or Probable when scored
    /// candidates reach the threshold. Merged duplicates and the excluded beneficiary are ignored.
    /// The identity lookup is 100% certain even when the five-field key differs.
    /// </summary>
    Task<BeneficiaryMatchSearchResult> FindAsync(BeneficiaryIdentityKey key, bool isBirthDateEstimated,
        Guid? excludeBeneficiaryId, CancellationToken cancellationToken,
        IReadOnlyCollection<(Guid IdentityTypeId, string Value)>? identities = null);
}

public sealed record BeneficiaryMatchSearchResult(
    BeneficiaryMatchStatus Status,
    IReadOnlyList<BeneficiaryMatch> Matches)
{
    public static BeneficiaryMatchSearchResult NotFound { get; } =
        new(BeneficiaryMatchStatus.NotFound, Array.Empty<BeneficiaryMatch>());
}
