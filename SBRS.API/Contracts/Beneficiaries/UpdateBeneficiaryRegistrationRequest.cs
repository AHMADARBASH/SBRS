namespace SBRS.Contracts.Beneficiaries;

public sealed record UpdateBeneficiaryRegistrationRequest(
    string FirstName,
    string FatherName,
    string LastName,
    string MotherName,
    DateOnly BirthDate,
    bool IsBirthDateEstimated,

    // true (1) = Male, false (0) = Female; must be sent explicitly.
    bool? Gender,
    Guid AreaId,
    Guid NationalityId,
    Guid IdentityTypeId,
    string? IdentityValue);
