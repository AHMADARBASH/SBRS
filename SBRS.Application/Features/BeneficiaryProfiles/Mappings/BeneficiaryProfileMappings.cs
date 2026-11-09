using SBRS.Application.Features.BeneficiaryProfiles.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.BeneficiaryProfiles.Mappings;

public static class BeneficiaryProfileMappings
{
    /// <summary>
    /// The row lists are passed by the handler (profile, disabilities, chronic rows) because
    /// they live in their own tables; ids only — the frontend resolves names from the lookups.
    /// </summary>
    public static BeneficiaryProfileDto ToDto(this BeneficiaryProfileEntity profile,
        IReadOnlyList<BeneficiaryDisabilityDto> disabilities, IReadOnlyList<Guid> chronicDiseaseIds) =>
        new(
            profile.BeneficiaryId,
            profile.PhoneNumber1,
            profile.PhoneNumber2,
            profile.IsDisplaced,
            profile.DisplacementDate,
            profile.OldAreaId,
            profile.AccommodationTypeId,
            profile.IsDisabled,
            disabilities,
            chronicDiseaseIds,
            profile.BeneficiaryStatusId,
            profile.CurrentWork,
            profile.PreviousWork,
            profile.EducationTypeId,
            profile.EducationIssueId,
            profile.MaritalStatusId,
            profile.WivesNumber,
            profile.ChildrenNumber);

    public static BeneficiaryDisabilityDto ToDto(this BeneficiaryDisabilityEntity disability) =>
        new(disability.DisabilityTypeId, disability.ReasonId);
}
