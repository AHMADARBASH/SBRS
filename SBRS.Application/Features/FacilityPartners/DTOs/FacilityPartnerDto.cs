namespace SBRS.Application.Features.FacilityPartners.DTOs;

public sealed record FacilityPartnerDto(
    Guid Id,
    Guid FacilityId,
    Guid PartnerId,
    string PartnerArName,
    string PartnerEnName,
    string ProjectCode,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrent);