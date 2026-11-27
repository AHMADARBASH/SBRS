namespace SBRS.Application.Features.Beneficiaries.DTOs;

/// <summary>Registration step data of one beneficiary, including the primary identity.</summary>
public sealed record BeneficiaryRegistrationDto(
    Guid Id,
    int Code,
    string FullCode,
    string FirstName,
    string FatherName,
    string LastName,
    string MotherName,
    DateOnly BirthDate,
    bool IsBirthDateEstimated,
    bool Gender,
    DateOnly EnrollmentDate,
    Guid AreaId,
    Guid NationalityId,
    Guid RegistrationFacilityId,
    Guid? IdentityTypeId,
    string? IdentityTypeCode,
    string? IdentityValue);
