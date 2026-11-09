namespace SBRS.Domain.Constants;

/// <summary>Stable codes of the marital_statuses lookup (rules use these codes, never row ids).</summary>
public static class MaritalStatusCodes
{
    public const string Single = "SINGLE";
    public const string Married = "MARRIED";
    public const string Divorced = "DIVORCED";
    public const string Widowed = "WIDOWED";

    public static readonly string[] All = [Single, Married, Divorced, Widowed];
}
