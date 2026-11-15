using System.Text.RegularExpressions;

namespace SBRS.Application.Common.Validation;

public static class RegexValidation
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// True when the pattern is a valid .NET regular expression. Mirrors the check in
    /// <c>IdentityTypeEntity.SetValueRegex</c> so callers get a field-level 400 instead of a
    /// domain error from the handler.
    /// </summary>
    public static bool IsValidPattern(string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return false;

        try
        {
            _ = new Regex(pattern, RegexOptions.None, Timeout);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
