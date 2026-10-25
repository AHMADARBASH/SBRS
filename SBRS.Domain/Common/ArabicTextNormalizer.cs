using System.Text;

namespace SBRS.Domain.Common;

public static class ArabicTextNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value.Trim())
        {
            // tatweel, diacritics, dagger alef
            if (ch == '\u0640' || (ch >= '\u064B' && ch <= '\u065F') || ch == '\u0670') continue;

            sb.Append(ch switch
            {
                '\u0623' or '\u0625' or '\u0622' or '\u0671' => '\u0627', // أ إ آ ٱ -> ا
                '\u0649' => '\u064A',                                      // ى -> ي
                '\u0629' => '\u0647',                                      // ة -> ه
                >= '\u0660' and <= '\u0669' => (char)('0' + (ch - '\u0660')), // Arabic-Indic digits
                >= '\u06F0' and <= '\u06F9' => (char)('0' + (ch - '\u06F0')), // Persian digits
                _ => char.ToLowerInvariant(ch)
            });
        }

        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
