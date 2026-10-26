using System.Text;

namespace SBRS.Domain.Common;

public static class IdentityValueNormalizer
{
    /// <summary>
    /// Trims the value and converts Arabic-Indic and Persian digits to ASCII digits.
    /// null becomes the empty string (the UNREGISTERED identity has an empty value).
    /// Trimming first also keeps a trailing newline from slipping past a regex that ends with $.
    /// </summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value.Trim())
        {
            sb.Append(ch switch
            {
                >= '\u0660' and <= '\u0669' => (char)('0' + (ch - '\u0660')), // Arabic-Indic digits
                >= '\u06F0' and <= '\u06F9' => (char)('0' + (ch - '\u06F0')), // Persian digits
                _ => ch
            });
        }

        return sb.ToString();
    }
}
