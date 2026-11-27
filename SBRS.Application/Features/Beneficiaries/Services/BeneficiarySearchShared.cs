using FluentValidation;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Mappings;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.Beneficiaries.Services;

/// <summary>The six optional keys shared by both beneficiary search endpoints.</summary>
public interface IBeneficiarySearchKeys
{
    string? FullCode { get; }
    string? FirstName { get; }
    string? FatherName { get; }
    string? LastName { get; }
    string? MotherName { get; }
    DateOnly? BirthDate { get; }
}

public abstract class BeneficiarySearchKeysValidator<T> : AbstractValidator<T>
    where T : class, IBeneficiarySearchKeys
{
    private const int MinNameLength = 2;

    protected BeneficiarySearchKeysValidator()
    {
        RuleFor(x => x.FullCode).MaximumLength(50);

        // A name that normalizes to nothing is "not provided" (see the group rule below).
        // A provided name must still reach the minimum length after normalization.
        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .Must(ReachesMinimumLength).When(x => NormalizedLength(x.FirstName) > 0)
            .WithMessage("First name must be at least 2 characters after normalization.");
        RuleFor(x => x.FatherName)
            .MaximumLength(100)
            .Must(ReachesMinimumLength).When(x => NormalizedLength(x.FatherName) > 0)
            .WithMessage("Father name must be at least 2 characters after normalization.");
        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .Must(ReachesMinimumLength).When(x => NormalizedLength(x.LastName) > 0)
            .WithMessage("Last name must be at least 2 characters after normalization.");
        RuleFor(x => x.MotherName)
            .MaximumLength(100)
            .Must(ReachesMinimumLength).When(x => NormalizedLength(x.MotherName) > 0)
            .WithMessage("Mother name must be at least 2 characters after normalization.");

        RuleFor(x => x)
            .Must(HasAtLeastOneKey)
            .WithMessage("At least one search key is required.");
    }

    private static int NormalizedLength(string? value) =>
        ArabicTextNormalizer.Normalize(value).Length;

    private static bool ReachesMinimumLength(string? value) =>
        NormalizedLength(value) >= MinNameLength;

    private static bool HasAtLeastOneKey(T x) =>
        !string.IsNullOrWhiteSpace(x.FullCode) ||
        NormalizedLength(x.FirstName) > 0 ||
        NormalizedLength(x.FatherName) > 0 ||
        NormalizedLength(x.LastName) > 0 ||
        NormalizedLength(x.MotherName) > 0 ||
        x.BirthDate is not null;
}

/// <summary>Normalization and result assembly shared by both search endpoints.</summary>
public static class BeneficiarySearchHelper
{
    public const int MaxResults = 20;

    public static BeneficiarySearchCriteria BuildCriteria(IBeneficiarySearchKeys keys) =>
        new(NormalizeFullCode(keys.FullCode),
            NormalizeName(keys.FirstName),
            NormalizeName(keys.FatherName),
            NormalizeName(keys.LastName),
            NormalizeName(keys.MotherName),
            keys.BirthDate);

    public static BeneficiarySearchResultDto ToResult(
        IReadOnlyList<BeneficiaryEntity> matches,
        IReadOnlyDictionary<Guid, BeneficiaryIdentityEntity> identities,
        IReadOnlySet<Guid> enrolledIds) =>
        new(matches.Take(MaxResults)
                .Select(b => b.ToSearchItemDto(identities.RequireIdentity(b), enrolledIds.Contains(b.Id)))
                .ToList(),
            matches.Count > MaxResults);

    // Generated FullCodes are upper case; citext makes the database comparison case-insensitive too.
    private static string? NormalizeFullCode(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static string? NormalizeName(string? value)
    {
        var normalized = ArabicTextNormalizer.Normalize(value);
        return normalized.Length == 0 ? null : normalized;
    }
}
