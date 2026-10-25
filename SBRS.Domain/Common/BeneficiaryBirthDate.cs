namespace SBRS.Domain.Common;

public static class BeneficiaryBirthDate
{
    /// <summary>
    /// When the birth date is estimated, only the year counts: the stored date is 1 January of that year.
    /// Every place that builds an identity key or stores a birth date must call this first.
    /// </summary>
    public static DateOnly Resolve(DateOnly birthDate, bool isEstimated) =>
        isEstimated ? new DateOnly(birthDate.Year, 1, 1) : birthDate;
}
