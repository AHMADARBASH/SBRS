using System.Text.RegularExpressions;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Constants;

public static class IdentityValuePatterns
{
    private const string Flexible = @"^[\p{L}\p{N}/-]{1,30}$";

    private static readonly Dictionary<string, Regex> Patterns = new()
    {
        [IdentityTypeCodes.FamilyBook] = Build(Flexible),
        [IdentityTypeCodes.VaccinationCard] = Build(Flexible),
        [IdentityTypeCodes.Passport] = Build(@"^[A-Za-z0-9/-]{1,20}$"),
        // [0-9] instead of \d: in .NET \d also matches Arabic-Indic digits
        [IdentityTypeCodes.NationalId] = Build(@"^[0-9]{10}$"),
        [IdentityTypeCodes.CivilRegistry] = Build(Flexible),
        [IdentityTypeCodes.DrivingLicense] = Build(Flexible),
        [IdentityTypeCodes.BirthCertificate] = Build(Flexible),
        [IdentityTypeCodes.DisabilityCertificate] = Build(Flexible),
        [IdentityTypeCodes.FamilyStatement] = Build(Flexible),
        [IdentityTypeCodes.MarriageCertificate] = Build(Flexible),
        [IdentityTypeCodes.Unregistered] = Build(@"^$")
    };

    private static Regex Build(string pattern) =>
        new(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(100));

    public static bool IsKnown(string code) => Patterns.ContainsKey(code);

    /// Returns the trimmed value, or throws DomainException if it doesn't match the type's pattern.
    public static string Validate(string code, string? value)
    {
        if (!Patterns.TryGetValue(code, out var regex))
            throw new DomainException($"Identity type '{code}' has no validation pattern.");

        var trimmed = (value ?? string.Empty).Trim();

        if (!regex.IsMatch(trimmed))
            throw new DomainException(code == IdentityTypeCodes.Unregistered
                ? "The identity value must be empty for an unregistered beneficiary."
                : $"The identity value is not valid for identity type '{code}'.");

        return trimmed;
    }
}