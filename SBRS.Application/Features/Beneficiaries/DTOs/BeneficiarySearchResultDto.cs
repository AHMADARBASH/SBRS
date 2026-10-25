using SBRS.Domain.Enums;

namespace SBRS.Application.Features.Beneficiaries.DTOs;

public sealed record BeneficiarySearchResultDto(
    BeneficiaryMatchStatus Status,
    int MatchCount,
    IReadOnlyList<BeneficiaryMatchDto> Matches);
