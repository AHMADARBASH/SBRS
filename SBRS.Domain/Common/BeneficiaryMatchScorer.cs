using SBRS.Domain.Entities;
using SBRS.Domain.Enums;

namespace SBRS.Domain.Common;

public static class BeneficiaryMatchScorer
{
    public const int ProbableThreshold = 70;
    private const double MinNameSimilarity = 0.75;

    // Weights add up to 100.
    private const int FirstNameWeight = 25;
    private const int FatherNameWeight = 15;
    private const int LastNameWeight = 20;
    private const int MotherNameWeight = 25;
    private const int BirthDateWeight = 15;

    public static BeneficiaryMatch Evaluate(BeneficiaryIdentityKey input, bool isInputBirthDateEstimated,
        BeneficiaryEntity candidate)
    {
        // Same five normalized values: certain match.
        if (candidate.MatchKey == input.MatchKey)
        {
            return new BeneficiaryMatch(candidate, BeneficiaryMatchStatus.Exact, 100,
                BeneficiaryMatchReasons.SameFirstName | BeneficiaryMatchReasons.SameFatherName |
                BeneficiaryMatchReasons.SameLastName | BeneficiaryMatchReasons.SameMotherName |
                BeneficiaryMatchReasons.SameBirthDate);
        }

        var reasons = BeneficiaryMatchReasons.None;
        var score = 0;

        score += ScoreName(input.FirstName, candidate.NormalizedFirstName, FirstNameWeight,
            BeneficiaryMatchReasons.SameFirstName, ref reasons);
        score += ScoreName(input.FatherName, candidate.NormalizedFatherName, FatherNameWeight,
            BeneficiaryMatchReasons.SameFatherName, ref reasons);
        score += ScoreName(input.LastName, candidate.NormalizedLastName, LastNameWeight,
            BeneficiaryMatchReasons.SameLastName, ref reasons);
        score += ScoreName(input.MotherName, candidate.NormalizedMotherName, MotherNameWeight,
            BeneficiaryMatchReasons.SameMotherName, ref reasons);

        if (input.BirthDate == candidate.BirthDate)
        {
            score += BirthDateWeight;
            reasons |= BeneficiaryMatchReasons.SameBirthDate;
        }
        else if ((isInputBirthDateEstimated || candidate.IsBirthDateEstimated)
                 && input.BirthDate.Year == candidate.BirthDate.Year)
        {
            score += BirthDateWeight / 2;
            reasons |= BeneficiaryMatchReasons.SameBirthYear;
        }

        // Possible twins: same mother, last name and birth date, different first name.
        // Always shown to a human, never auto-resolved.
        var twinPattern =
            input.MotherName == candidate.NormalizedMotherName &&
            input.LastName == candidate.NormalizedLastName &&
            input.BirthDate == candidate.BirthDate &&
            input.FirstName != candidate.NormalizedFirstName;

        if (twinPattern)
        {
            reasons |= BeneficiaryMatchReasons.SameMotherLastNameBirthDate;
            score = Math.Max(score, ProbableThreshold);
        }

        var status = score >= ProbableThreshold ? BeneficiaryMatchStatus.Probable : BeneficiaryMatchStatus.NotFound;
        return new BeneficiaryMatch(candidate, status, Math.Min(score, 99), reasons);
    }

    private static int ScoreName(string input, string candidate, int weight,
        BeneficiaryMatchReasons sameFlag, ref BeneficiaryMatchReasons reasons)
    {
        if (input.Length == 0 || candidate.Length == 0) return 0;

        if (input == candidate)
        {
            reasons |= sameFlag;
            return weight;
        }

        var similarity = NameSimilarity.Similarity(input, candidate);
        if (similarity < MinNameSimilarity) return 0;

        reasons |= BeneficiaryMatchReasons.SimilarSpelling;
        return (int)Math.Round(weight * similarity);
    }
}
