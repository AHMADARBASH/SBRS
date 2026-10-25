namespace SBRS.Domain.Enums;

public enum BeneficiaryMatchReviewStatus : byte
{
    NoMatchFound = 0,
    PendingReview = 1,
    ConfirmedDistinct = 2,
    MergedIntoExisting = 3
}
