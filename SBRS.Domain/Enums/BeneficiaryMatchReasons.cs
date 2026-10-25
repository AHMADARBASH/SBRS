namespace SBRS.Domain.Enums;

[Flags]
public enum BeneficiaryMatchReasons
{
    None = 0,
    SameFirstName = 1,
    SameFatherName = 2,
    SameLastName = 4,
    SameMotherName = 8,
    SameBirthDate = 16,
    SameBirthYear = 32,          // only when one of the dates is estimated
    SimilarSpelling = 64,        // at least one name matched by edit distance, not equality
    SameMotherLastNameBirthDate = 128 // possible twins: first name differs
}
