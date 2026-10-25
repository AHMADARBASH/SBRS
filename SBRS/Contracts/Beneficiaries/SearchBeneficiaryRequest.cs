namespace SBRS.Contracts.Beneficiaries;

public sealed record SearchBeneficiaryRequest(
    string FirstName,
    string FatherName,
    string LastName,
    string MotherName,
    DateOnly BirthDate,
    bool IsBirthDateEstimated);
