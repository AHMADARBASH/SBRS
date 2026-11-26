using System.Text.RegularExpressions;
using FluentValidation;
using SBRS.Domain.Common;
using SBRS.Domain.Constants;

namespace SBRS.Application.Common.Validation;

// Shared form rules (names, work text, Syrian phones, dates, marital counts). One place only.
public static class BeneficiaryFormRules
{
    public const int PersonNameMaxLength = 100;
    public const int WorkTextMaxLength = 200;
    public const int MaxDisabilities = 20;
    public const int MaxChronicDiseases = 30;

    private static readonly Regex PersonNamePattern =
        new(@"^[\p{L}\p{M}]+(?: [\p{L}\p{M}]+)*$", RegexOptions.Compiled);

    private static readonly Regex DigitsOnly = new(@"^[0-9]+$", RegexOptions.Compiled);
    private static readonly Regex LocalMobile = new(@"^09[0-9]{8}$", RegexOptions.Compiled);
    private static readonly Regex LocalLandline = new(@"^0[1-8][0-9]{7,8}$", RegexOptions.Compiled);
    private static readonly Regex CountryMobile = new(@"^9[0-9]{8}$", RegexOptions.Compiled);
    private static readonly Regex CountryLandline = new(@"^[1-8][0-9]{7,8}$", RegexOptions.Compiled);

    public static bool IsValidPersonName(string? value, int maxLength = PersonNameMaxLength)
    {
        var trimmed = value?.Trim();
        return trimmed is { Length: >= 2 } && trimmed.Length <= maxLength &&
               trimmed.Any(char.IsLetter) && PersonNamePattern.IsMatch(trimmed);
    }

    public static bool IsValidWorkText(string? value, int maxLength = WorkTextMaxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;   // optional: empty is null
        var trimmed = value.Trim();
        return trimmed.Length >= 2 && trimmed.Length <= maxLength &&
               trimmed.Any(char.IsLetter) && PersonNamePattern.IsMatch(trimmed);
    }

    // Returns true for empty input (optional field) with a null canonical value.
    // Accepted: local mobile/landline, and +963 / 00963 / 963 followed by the local number without 0.
    // The canonical value is always the local form starting with 0.
    public static bool TryNormalizePhone(string? value, out string? canonical)
    {
        canonical = null;
        if (string.IsNullOrWhiteSpace(value)) return true;

        var normalized = ConvertDigits(value.Trim()).Replace(" ", string.Empty).Replace("-", string.Empty);

        if (normalized.StartsWith("+", StringComparison.Ordinal))
            return normalized.StartsWith("+963", StringComparison.Ordinal) &&
                   TryCanonicalCountry(normalized.Substring(4), out canonical);

        if (normalized.StartsWith("00963", StringComparison.Ordinal))
            return TryCanonicalCountry(normalized.Substring(5), out canonical);

        if (normalized.StartsWith("963", StringComparison.Ordinal))
            return TryCanonicalCountry(normalized.Substring(3), out canonical);

        if (LocalMobile.IsMatch(normalized) || LocalLandline.IsMatch(normalized))
        {
            canonical = normalized;
            return true;
        }

        return false;
    }

    private static bool TryCanonicalCountry(string rest, out string? canonical)
    {
        canonical = null;
        if (!DigitsOnly.IsMatch(rest)) return false;
        if (!CountryMobile.IsMatch(rest) && !CountryLandline.IsMatch(rest)) return false;

        canonical = "0" + rest;
        return true;
    }

    private static string ConvertDigits(string value) =>
        new(value.Select(c => c switch
        {
            >= '\u0660' and <= '\u0669' => (char)(c - '\u0660' + '0'),   // Arabic-Indic
            >= '\u06F0' and <= '\u06F9' => (char)(c - '\u06F0' + '0'),   // Persian
            _ => c
        }).ToArray());

    public static bool IsValidBirthDate(DateOnly birthDate, bool isEstimated)
    {
        var resolved = BeneficiaryBirthDate.Resolve(birthDate, isEstimated);
        return resolved >= new DateOnly(1900, 1, 1) && resolved <= DateOnly.FromDateTime(DateTime.UtcNow);
    }

    public static bool IsValidEnrollmentDate(DateOnly birthDate, DateOnly enrollmentDate, bool birthDateEstimated)
    {
        var resolvedBirthDate = BeneficiaryBirthDate.Resolve(birthDate, birthDateEstimated);
        return enrollmentDate >= resolvedBirthDate && enrollmentDate <= DateOnly.FromDateTime(DateTime.UtcNow);
    }

    public static bool IsValidDisplacementDate(DateOnly? displacementDate) =>
        displacementDate is null || displacementDate <= DateOnly.FromDateTime(DateTime.UtcNow);

    public static bool AllowsWivesNumber(string? maritalStatusCode, bool isMale) =>
        string.Equals(maritalStatusCode, MaritalStatusCodes.Married, StringComparison.OrdinalIgnoreCase) && isMale;

    public static bool AllowsChildrenNumber(string? maritalStatusCode) =>
        string.Equals(maritalStatusCode, MaritalStatusCodes.Married, StringComparison.OrdinalIgnoreCase);
}

public static class BeneficiaryFormRuleExtensions
{
    public static IRuleBuilderOptions<T, string> PersonName<T>(this IRuleBuilder<T, string> rule,
        string label, int maxLength = BeneficiaryFormRules.PersonNameMaxLength) =>
        rule.Must(v => BeneficiaryFormRules.IsValidPersonName(v, maxLength))
            .WithMessage($"{label} must be 2-{maxLength} letters and single spaces only.");

    public static IRuleBuilderOptions<T, string?> WorkText<T>(this IRuleBuilder<T, string?> rule, string label) =>
        rule.Must(v => BeneficiaryFormRules.IsValidWorkText(v))
            .WithMessage($"{label} must be letters and single spaces only (2-200 characters).");

    public static IRuleBuilderOptions<T, string?> SyrianPhone<T>(this IRuleBuilder<T, string?> rule, string label) =>
        rule.Must(v => BeneficiaryFormRules.TryNormalizePhone(v, out _))
            .WithMessage($"{label} is not a valid Syrian phone number.");
}

// Canonical local phone (starts with 0, no country code). Empty becomes null.
public static class PhoneNumberNormalizer
{
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return BeneficiaryFormRules.TryNormalizePhone(value, out var canonical) ? canonical : value.Trim();
    }
}
