namespace SBRS.Contracts.Visits;

public sealed record UpdateVisitRequest(
    Guid Id,
    DateOnly? VisitDate,
    decimal? Weight,
    decimal? Height,
    bool? IsPregnant);
