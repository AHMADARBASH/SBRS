namespace SBRS.Contracts.Beneficiaries;

/// <summary>
/// Body of PUT {beneficiaryId}/Profile (full replace): everything must be sent every time.
/// All entity lookups and lists carry ids only; the frontend resolves names via the lookup endpoints.
/// </summary>
public sealed record SaveBeneficiaryProfileRequest(
    string? PhoneNumber1,
    string? PhoneNumber2,
    bool IsDisplaced,
    DateOnly? DisplacementDate,
    Guid? OldAreaId,
    Guid? AccommodationTypeId,          // optional: null while the answer is not known yet
    bool IsDisabled,
    IReadOnlyList<SaveBeneficiaryProfileDisabilityRequest> Disabilities,
    IReadOnlyList<Guid>? ChronicDiseaseIds,
    Guid? BeneficiaryStatusId,
    string? CurrentWork,
    string? PreviousWork,
    Guid? EducationTypeId,              // optional: null while the answer is not known yet
    Guid? EducationIssueId,
    Guid? MaritalStatusId,              // optional: null while the answer is not known yet
    short? WivesNumber,
    short? ChildrenNumber);

/// <summary>One disability row: which type it is and the reason id (both required).</summary>
public sealed record SaveBeneficiaryProfileDisabilityRequest(Guid DisabilityTypeId, Guid ReasonId);
