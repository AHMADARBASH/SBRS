namespace SBRS.Domain.Common;

/// <summary>
/// The five matching fields after normalization, plus their hash.
/// Every place that compares or stores identity values must build it through From().
/// </summary>
public sealed record BeneficiaryIdentityKey(
    string FirstName,
    string FatherName,
    string LastName,
    string MotherName,
    DateOnly BirthDate,
    string MatchKey)
{
    public static BeneficiaryIdentityKey From(string firstName, string fatherName, string lastName,
        string motherName, DateOnly birthDate)
    {
        var first = ArabicTextNormalizer.Normalize(firstName);
        var father = ArabicTextNormalizer.Normalize(fatherName);
        var last = ArabicTextNormalizer.Normalize(lastName);
        var mother = ArabicTextNormalizer.Normalize(motherName);

        return new BeneficiaryIdentityKey(first, father, last, mother, birthDate,
            BeneficiaryMatchKey.Build(first, father, last, mother, birthDate));
    }
}
