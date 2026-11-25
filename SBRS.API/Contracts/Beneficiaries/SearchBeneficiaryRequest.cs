namespace SBRS.Contracts.Beneficiaries;

/// <summary>
/// Cross-facility beneficiary search. Every key is optional on its own, but at least one
/// key with a real value is required; all provided keys are combined with AND.
/// </summary>
/// <param name="FullCode">
/// Exact full code, compared case-insensitively. A code belonging to a merged duplicate
/// resolves to the surviving beneficiary.
/// </param>
/// <param name="FirstName">First name; starts-with after Arabic normalization, minimum 2 normalized characters.</param>
/// <param name="FatherName">Father name (the second name); starts-with after Arabic normalization.</param>
/// <param name="LastName">Last name; starts-with after Arabic normalization.</param>
/// <param name="MotherName">Mother's first name; starts-with after Arabic normalization. Filter only, never returned.</param>
/// <param name="BirthDate">Exact birth date (date only, no time part).</param>
public sealed record SearchBeneficiaryRequest(
    string? FullCode,
    string? FirstName,
    string? FatherName,
    string? LastName,
    string? MotherName,
    DateOnly? BirthDate);
