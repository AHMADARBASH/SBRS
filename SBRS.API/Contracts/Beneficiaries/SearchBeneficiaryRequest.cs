namespace SBRS.Contracts.Beneficiaries;

public sealed record SearchBeneficiaryRequest(string? FullCode, string? FirstName, string? LastName);
