namespace SBRS.Contracts.FacilityPartners;

public sealed record AssignFacilityPartnerRequest(
    Guid FacilityId,
    Guid PartnerId,
    string? ProjectCode,
    DateOnly StartDate);