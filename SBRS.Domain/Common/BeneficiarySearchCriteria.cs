namespace SBRS.Domain.Common;

/// <summary>
/// The normalized values of one cross-facility beneficiary search. Every key is optional;
/// a null key is ignored, and the provided keys are combined with AND by the repository.
/// Names must already be normalized with <see cref="ArabicTextNormalizer"/>.
/// </summary>
public sealed record BeneficiarySearchCriteria(
    string? FullCode,
    string? NormalizedFirstName,
    string? NormalizedFatherName,
    string? NormalizedLastName,
    string? NormalizedMotherName,
    DateOnly? BirthDate);
