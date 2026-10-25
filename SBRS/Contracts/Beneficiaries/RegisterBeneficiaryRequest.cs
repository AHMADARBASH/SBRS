using SBRS.Domain.Enums;

namespace SBRS.Contracts.Beneficiaries;

public sealed record RegisterBeneficiaryRequest(
    string FirstName,
    string FatherName,
    string LastName,
    string MotherName,
    DateOnly BirthDate,
    bool IsBirthDateEstimated,

    /// <summary>true (1) = Male, false (0) = Female.</summary>
    bool Gender,
    DateOnly EnrollmentDate,
    Guid AreaId);
