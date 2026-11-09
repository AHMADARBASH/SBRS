namespace SBRS.Domain.Constants;

/// <summary>Stable codes of the beneficiary_statuses lookup (rules use these codes, never row ids).</summary>
public static class BeneficiaryStatusCodes
{
    /// <summary>Default status when a profile is saved without one.</summary>
    public const string NotAvailable = "NOT_AVAILABLE";

    public const string FormerDetainee = "FORMER_DETAINEE";
    public const string Missing = "MISSING";
}
