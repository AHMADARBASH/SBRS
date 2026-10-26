namespace SBRS.Application.Features.Beneficiaries.DTOs;

public sealed record BeneficiaryIdentityDto(
    Guid Id,
    Guid IdentityTypeId,
    string IdentityTypeCode,
    string IdentityTypeArName,
    string IdentityTypeEnName,
    string Value);
