namespace SBRS.Domain.Common;


public sealed record BeneficiarySearchCriteria(
    string? FullCode,
    string? NormalizedFirstName,
    string? NormalizedFatherName,
    string? NormalizedLastName,
    string? NormalizedMotherName,
    DateOnly? BirthDate);
