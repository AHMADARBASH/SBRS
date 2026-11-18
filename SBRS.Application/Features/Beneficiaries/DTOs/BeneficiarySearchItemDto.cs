using SBRS.Domain.Enums;

namespace SBRS.Application.Features.Beneficiaries.DTOs;

public sealed record BeneficiarySearchItemDto(
    Guid Id,
    string FullCode,
    string FirstName,
    string FatherName,
    string LastName,
    DateOnly BirthDate,
    bool IsBirthDateEstimated,

    /// <summary>true (1) = Male, false (0) = Female.</summary>
    bool Gender,

    /// <summary>The type of the beneficiary's identity; the value is not shown (cross-facility search).</summary>
    string IdentityTypeCode,

    /// <summary>True when the caller's facility already has an active enrollment (false for SystemAdmin: no facility).</summary>
    bool IsEnrolledInMyFacility);
