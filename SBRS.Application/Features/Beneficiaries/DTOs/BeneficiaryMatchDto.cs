using SBRS.Domain.Enums;

namespace SBRS.Application.Features.Beneficiaries.DTOs;

public sealed record BeneficiaryMatchDto(
    Guid BeneficiaryId,
    string FullCode,
    string FirstName,
    string FatherName,
    string LastName,
    string MotherName,
    DateOnly BirthDate,
    bool IsBirthDateEstimated,

    /// <summary>true (1) = Male, false (0) = Female.</summary>
    bool Gender,
    BeneficiaryMatchStatus Status,
    int Score,
    BeneficiaryMatchReasons Reasons,
    BeneficiaryIdentityDto Identity);
