namespace SBRS.Application.Features.Visits.DTOs;

public sealed record VisitDto(
    Guid Id,
    Guid FacilityId,
    Guid BeneficiaryId,
    string BeneficiaryFullCode,
    string BeneficiaryFirstName,
    string BeneficiaryLastName,
    DateOnly VisitDate,
    decimal? Weight,
    decimal? Height,
    bool? IsPregnant);
