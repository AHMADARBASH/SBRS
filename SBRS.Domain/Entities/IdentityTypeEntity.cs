using System.Text.RegularExpressions;
using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class IdentityTypeEntity : BaseEntity
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);

    private IdentityTypeEntity() { }   // EF

    public IdentityTypeEntity(string arName, string enName, string code, string valueRegex = "")
    {
        Set(arName, enName, code);
        if (!string.IsNullOrEmpty(valueRegex)) SetValueRegex(valueRegex);
    }

    public string ArName { get; private set; } = default!;
    public string EnName { get; private set; } = default!;
    public string Code { get; private set; } = default!;

    /// <summary>.NET regex the identity value must match (after normalization).</summary>
    public string ValueRegex { get; private set; } = string.Empty;

    public void SetValueRegex(string valueRegex)
    {
        if (string.IsNullOrEmpty(valueRegex)) throw new DomainException("Value regex is required.");

        try { _ = new Regex(valueRegex, RegexOptions.None, RegexTimeout); }
        catch (ArgumentException) { throw new DomainException("Value regex is not a valid regular expression."); }

        ValueRegex = valueRegex;
    }

    /// <summary>The value must already be normalized with IdentityValueNormalizer.</summary>
    public bool IsValidValue(string normalizedValue)
    {
        if (string.IsNullOrEmpty(ValueRegex)) return false;   // a type without a rule cannot be used

        try { return Regex.IsMatch(normalizedValue, ValueRegex, RegexOptions.None, RegexTimeout); }
        catch (RegexMatchTimeoutException) { return false; }
    }

    public void Update(string arName, string enName, string code) => Set(arName, enName, code);

    private void Set(string arName, string enName, string code)
    {
        if (string.IsNullOrWhiteSpace(arName) || string.IsNullOrWhiteSpace(enName) || string.IsNullOrWhiteSpace(code))
            throw new DomainException("Arabic name, English name and code are required.");

        ArName = arName.Trim();
        EnName = enName.Trim();
        Code = code.Trim();
    }
}
