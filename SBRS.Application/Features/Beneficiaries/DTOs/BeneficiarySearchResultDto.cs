namespace SBRS.Application.Features.Beneficiaries.DTOs;

public sealed record BeneficiarySearchResultDto(
    IReadOnlyList<BeneficiarySearchItemDto> Items,
    bool HasMore);
