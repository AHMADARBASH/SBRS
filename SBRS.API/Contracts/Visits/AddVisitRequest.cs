namespace SBRS.Contracts.Visits;

public sealed record AddVisitRequest(
    Guid BeneficiaryId,
    DateOnly? VisitDate,
    decimal? Weight,
    decimal? Height,
    bool? IsPregnant);
