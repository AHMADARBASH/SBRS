using SBRS.Domain.Entities;

namespace SBRS.Application.Features.BeneficiaryProfiles.DTOs;

/// <summary>
/// The full profile of one beneficiary. Ids only: the frontend resolves the display
/// names through the lookup endpoints. Sensitive data — never included in list results.
/// </summary>
public sealed record BeneficiaryProfileDto(
    Guid BeneficiaryId,
    string? PhoneNumber1,
    string? PhoneNumber2,
    bool IsDisplaced,
    DateOnly? DisplacementDate,
    Guid? OldAreaId,
    Guid AccommodationTypeId,
    bool IsDisabled,
    IReadOnlyList<BeneficiaryDisabilityDto> Disabilities,
    IReadOnlyList<Guid> ChronicDiseaseIds,
    Guid BeneficiaryStatusId,
    string? CurrentWork,
    string? PreviousWork,
    Guid EducationTypeId,
    Guid? EducationIssueId,
    Guid MaritalStatusId,
    short? WivesNumber,
    short? ChildrenNumber);

public sealed record BeneficiaryDisabilityDto(Guid DisabilityTypeId, Guid ReasonId);

/// <summary>What the save handler reports so the controller can answer 201 (create) or 200 (update).</summary>
public sealed record SaveBeneficiaryProfileResult(BeneficiaryProfileDto Profile, bool Created);
