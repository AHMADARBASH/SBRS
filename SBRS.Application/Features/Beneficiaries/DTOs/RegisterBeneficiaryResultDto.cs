using SBRS.Domain.Enums;

namespace SBRS.Application.Features.Beneficiaries.DTOs;

public sealed record RegisterBeneficiaryResultDto(
    Guid Id,
    int Code,
    string FullCode,
    BeneficiaryMatchReviewStatus MatchReviewStatus);
