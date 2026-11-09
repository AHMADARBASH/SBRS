using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.BeneficiaryProfiles.DTOs;
using SBRS.Application.Features.BeneficiaryProfiles.Mappings;
using SBRS.Domain.Constants;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryProfiles.Commands;

public sealed record SaveBeneficiaryDisabilityItem(Guid DisabilityTypeId, Guid ReasonId);

/// <summary>Upsert of the sensitive profile: create when none exists, full replace otherwise.</summary>
public sealed record SaveBeneficiaryProfileCommand(
    Guid BeneficiaryId,
    string? PhoneNumber1,
    string? PhoneNumber2,
    bool IsDisplaced,
    DateOnly? DisplacementDate,
    Guid? OldAreaId,
    Guid? AccommodationTypeId,          // nullable: the answer may not be known at save time
    bool IsDisabled,
    IReadOnlyList<SaveBeneficiaryDisabilityItem>? Disabilities,
    IReadOnlyList<Guid>? ChronicDiseaseIds,
    Guid? BeneficiaryStatusId,
    string? CurrentWork,
    string? PreviousWork,
    Guid? EducationTypeId,              // nullable: the answer may not be known at save time
    Guid? EducationIssueId,
    Guid? MaritalStatusId,              // nullable: the answer may not be known at save time
    short? WivesNumber,
    short? ChildrenNumber) : IRequest<SaveBeneficiaryProfileResult>;

/// <summary>Arabic-Indic / Eastern-Arabic-Indic digits to ASCII, trim, empty is null (matches «phone» rules).</summary>
public static class PhoneNumberNormalizer
{
    public static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;

        return new string(trimmed.Select(c => c switch
        {
            >= '\u0660' and <= '\u0669' => (char)(c - '\u0660' + '0'),   // Arabic-Indic ٠..٩
            >= '\u06F0' and <= '\u06F9' => (char)(c - '\u06F0' + '0'),   // Eastern ۰..۹
            _ => c,
        }).ToArray());
    }
}

public sealed class SaveBeneficiaryProfileCommandValidator : AbstractValidator<SaveBeneficiaryProfileCommand>
{
    public SaveBeneficiaryProfileCommandValidator()
    {
        RuleFor(x => x.BeneficiaryId).NotEmpty();
        // nullable lookups: omitted = "not known yet", but a sent id must be a real one
        RuleFor(x => x.AccommodationTypeId).NotEmpty().When(x => x.AccommodationTypeId.HasValue);
        RuleFor(x => x.EducationTypeId).NotEmpty().When(x => x.EducationTypeId.HasValue);
        RuleFor(x => x.MaritalStatusId).NotEmpty().When(x => x.MaritalStatusId.HasValue);

        RuleFor(x => x.BeneficiaryStatusId).NotEmpty()
            .When(x => x.BeneficiaryStatusId.HasValue);   // null falls back to NOT_AVAILABLE
        RuleFor(x => x.EducationIssueId).NotEmpty()
            .When(x => x.EducationIssueId.HasValue);

        // phones: optional, converted, format checked; phone 2 needs phone 1 and must differ
        RuleFor(x => x.PhoneNumber1).MaximumLength(20)
            .Must(v => v is null || MatchesPhonePattern(v))
            .WithMessage("Phone number 1 is not valid.");
        RuleFor(x => x.PhoneNumber2).MaximumLength(20)
            .Must(v => v is null || MatchesPhonePattern(v))
            .WithMessage("Phone number 2 is not valid.");
        RuleFor(x => x).Must(x => string.IsNullOrWhiteSpace(x.PhoneNumber2) ||
                               !string.IsNullOrWhiteSpace(x.PhoneNumber1))
            .WithMessage("Phone number 2 cannot be set without phone number 1.");
        RuleFor(x => x).Must(x =>
            {
                var (p1, p2) = (PhoneNumberNormalizer.Normalize(x.PhoneNumber1),
                                PhoneNumberNormalizer.Normalize(x.PhoneNumber2));
                return p2 is null || p1 is null || p1 != p2;
            })
            .WithMessage("Phone number 2 must differ from phone number 1.");

        // displacement: nothing may remain when not displaced; future date rejected
        RuleFor(x => x.DisplacementDate).Null().When(x => !x.IsDisplaced)
            .WithMessage("A beneficiary that is not displaced cannot have a displacement date.");
        RuleFor(x => x.OldAreaId).Null().When(x => !x.IsDisplaced)
            .WithMessage("A beneficiary that is not displaced cannot have an old area.");
        RuleFor(x => x.OldAreaId).NotEmpty().When(x => x.IsDisplaced)
            .WithMessage("Old area is required for a displaced beneficiary.");
        RuleFor(x => x.DisplacementDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.IsDisplaced && x.DisplacementDate.HasValue)
            .WithMessage("Displacement date cannot be in the future.");
        // "not before the beneficiary's birth date" needs the beneficiary row: the handler checks it.

        RuleFor(x => x.CurrentWork).MaximumLength(200);
        RuleFor(x => x.PreviousWork).MaximumLength(200);

        // counts: the marital-status and gender rules need the lookup code (handler);
        // only the ranges are structural
        RuleFor(x => x.WivesNumber).InclusiveBetween((short)1, (short)4)
            .When(x => x.WivesNumber.HasValue)
            .WithMessage("Wives number must be between 1 and 4.");
        RuleFor(x => x.ChildrenNumber).InclusiveBetween((short)0, (short)30)
            .When(x => x.ChildrenNumber.HasValue)
            .WithMessage("Children number must be between 0 and 30.");

        // disabilities: rows only while disabled, each with type and reason, no duplicate types
        RuleFor(x => x.Disabilities).NotEmpty().When(x => x.IsDisabled)
            .WithMessage("At least one disability is required when the beneficiary is disabled.");
        RuleForEach(x => x.Disabilities).ChildRules(d =>
        {
            d.RuleFor(i => i.DisabilityTypeId).NotEmpty();
            d.RuleFor(i => i.ReasonId).NotEmpty();
        });
        RuleFor(x => x.Disabilities).Must(d => d is null ||
                d.Select(i => i.DisabilityTypeId).Distinct().Count() == d.Count)
            .WithMessage("The same disability type cannot appear twice.");
        RuleFor(x => x.Disabilities).Must(d => d is null || d.Count == 0).When(x => !x.IsDisabled)
            .WithMessage("Disabilities can only be reported for a disabled beneficiary.");

        // chronic diseases: optional, ids distinct (existence is checked by the handler)
        RuleFor(x => x.ChronicDiseaseIds).Must(ids => ids is null ||
                ids.Distinct().Count() == ids.Count)
            .WithMessage("The same chronic disease cannot appear twice.");
    }

    private static bool MatchesPhonePattern(string value)
    {
        var normalized = PhoneNumberNormalizer.Normalize(value);
        if (normalized is null) return true;   // empty resolves to null (the handler drops it)
        return System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^\+?[0-9]{7,15}$");
    }
}

public sealed class SaveBeneficiaryProfileCommandHandler(
    IBeneficiaryRepository beneficiaries,
    IBeneficiaryProfileRepository profiles,
    IGeographyRepository geography,
    IAccommodationTypeRepository accommodationTypes,
    IEducationTypeRepository educationTypes,
    IEducationIssueRepository educationIssues,
    IDisabilityTypeRepository disabilityTypes,
    IDisabilityReasonRepository disabilityReasons,
    IChronicDiseaseRepository chronicDiseases,
    IBeneficiaryStatusRepository statuses,
    IMaritalStatusRepository maritalStatuses,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SaveBeneficiaryProfileCommand, SaveBeneficiaryProfileResult>
{
    /// <summary>
    /// Marital statuses whose Code may report relationship counts. Extend this single place
    /// (e.g. divorced) instead of adding new if-chains — the rules key on the stable Code.
    /// </summary>
    private static readonly HashSet<string> StatusesAllowingChildrenNumber = [MaritalStatusCodes.Married];
    private static readonly HashSet<string> StatusesAllowingWivesNumber = [MaritalStatusCodes.Married];

    public async Task<SaveBeneficiaryProfileResult> Handle(SaveBeneficiaryProfileCommand command,
        CancellationToken cancellationToken)
    {
        var beneficiary = await beneficiaries.GetByIdAsync(command.BeneficiaryId, cancellationToken)
            ?? throw new NotFoundException(nameof(BeneficiaryEntity), command.BeneficiaryId);

        // Out-of-scope and deactivated/merged records look like missing ones (same rule as the other features).
        if (!currentUser.IsSystemAdmin && beneficiary.RegistrationFacilityId != currentUser.FacilityId ||
            !beneficiary.IsActive || beneficiary.MergedIntoBeneficiaryId is not null)
            throw new NotFoundException(nameof(BeneficiaryEntity), command.BeneficiaryId);

        // Lookup existence: a sent id must exist (400); omitted simply stays null ("not known yet").
        var marital = command.MaritalStatusId is { } maritalId
            ? await maritalStatuses.GetByIdAsync(maritalId, cancellationToken)
                ?? throw new DomainException("The selected marital status does not exist.")
            : null;

        BeneficiaryStatusEntity status;
        if (command.BeneficiaryStatusId is { } statusId)
        {
            status = await statuses.GetByIdAsync(statusId, cancellationToken)
                ?? throw new DomainException("The selected beneficiary status does not exist.");
        }
        else
        {
            status = await statuses.GetByCodeAsync(BeneficiaryStatusCodes.NotAvailable, cancellationToken)
                ?? throw new DomainException(
                    "The default beneficiary status is missing; reference data must be seeded first.");
        }

        if (command.AccommodationTypeId is { } accommodationId)
            _ = await accommodationTypes.GetByIdAsync(accommodationId, cancellationToken)
                ?? throw new DomainException("The selected accommodation type does not exist.");
        if (command.EducationTypeId is { } educationId)
            _ = await educationTypes.GetByIdAsync(educationId, cancellationToken)
                ?? throw new DomainException("The selected education type does not exist.");

        if (command.EducationIssueId is { } issueId)
            _ = await educationIssues.GetByIdAsync(issueId, cancellationToken)
                ?? throw new DomainException("The selected education issue does not exist.");

        // Displacement must make sense for this person (the future-date check ran in the validator).
        if (command.IsDisplaced && command.DisplacementDate is { } displacedOn &&
            displacedOn < beneficiary.BirthDate)
            throw new DomainException("Displacement date cannot be before the beneficiary's birth date.");

        if (command.IsDisplaced && command.OldAreaId is { } oldAreaId)
            _ = await geography.GetAreaByIdAsync(oldAreaId, cancellationToken)
                ?? throw new DomainException("The selected old area does not exist.");

        ApplyMaritalFamilyRules(command, marital, beneficiary);

        var requestedDisabilities = command.Disabilities ?? [];
        await EnsureDisabilitiesExistAsync(requestedDisabilities, cancellationToken);

        var chronicIds = (command.ChronicDiseaseIds ?? []).ToList();
        await EnsureChronicDiseasesExistAsync(chronicIds, cancellationToken);

        // Phones arrive converted/trimmed; the entity trims again and enforces the same rules.
        var phone1 = PhoneNumberNormalizer.Normalize(command.PhoneNumber1);
        var phone2 = PhoneNumberNormalizer.Normalize(command.PhoneNumber2);

        var profile = await profiles.GetByBeneficiaryIdAsync(beneficiary.Id, cancellationToken);
        bool created;
        if (profile is null)
        {
            profile = new BeneficiaryProfileEntity(
                beneficiary.Id, phone1, phone2,
                command.IsDisplaced, command.DisplacementDate, command.OldAreaId,
                command.AccommodationTypeId, command.IsDisabled, status.Id,
                command.CurrentWork, command.PreviousWork,
                command.EducationTypeId, command.EducationIssueId,
                marital?.Id, command.WivesNumber, command.ChildrenNumber);
            await profiles.AddAsync(profile, cancellationToken);
            created = true;
        }
        else
        {
            profile.Update(
                phone1, phone2,
                command.IsDisplaced, command.DisplacementDate, command.OldAreaId,
                command.AccommodationTypeId, command.IsDisabled, status.Id,
                command.CurrentWork, command.PreviousWork,
                command.EducationTypeId, command.EducationIssueId,
                marital?.Id, command.WivesNumber, command.ChildrenNumber);
            created = false;
        }

        // Full-replace semantics by diff: profile + child rows in one SaveChangesAsync (one transaction).
        await SyncDisabilitiesAsync(beneficiary.Id, requestedDisabilities, cancellationToken);
        await SyncChronicDiseasesAsync(beneficiary.Id, chronicIds, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var savedDisabilities = requestedDisabilities
            .Select(d => new BeneficiaryDisabilityDto(d.DisabilityTypeId, d.ReasonId)).ToList();
        return new SaveBeneficiaryProfileResult(
            profile.ToDto(savedDisabilities, chronicIds), created);
    }

    /// <summary>
    /// Wives/children are allowed only for marital statuses keyed by Code (single extensible place),
    /// and wives additionally only for a male beneficiary. When the marital status itself is not
    /// known yet, sending a count is rejected (the rule cannot be evaluated) — never silently stored.
    /// </summary>
    private void ApplyMaritalFamilyRules(SaveBeneficiaryProfileCommand command,
        MaritalStatusEntity? marital, BeneficiaryEntity beneficiary)
    {
        if (marital is null)
        {
            if (command.WivesNumber is not null)
                throw new DomainException(
                    "Wives number requires the marital status to be known and MARRIED.");
            if (command.ChildrenNumber is not null)
                throw new DomainException(
                    "Children number requires the marital status to be known and MARRIED.");
            return;
        }

        if (command.WivesNumber is not null)
        {
            if (!StatusesAllowingWivesNumber.Contains(marital.Code) || !beneficiary.Gender)
                throw new DomainException("Wives number can only be set for a married male beneficiary.");
        }

        if (command.ChildrenNumber is not null && !StatusesAllowingChildrenNumber.Contains(marital.Code))
            throw new DomainException("Children number can only be set for a married beneficiary.");
    }

    private async Task EnsureDisabilitiesExistAsync(IReadOnlyList<SaveBeneficiaryDisabilityItem> items,
        CancellationToken ct)
    {
        foreach (var item in items)
        {
            _ = await disabilityTypes.GetByIdAsync(item.DisabilityTypeId, ct)
                ?? throw new DomainException("One of the selected disability types does not exist.");
            _ = await disabilityReasons.GetByIdAsync(item.ReasonId, ct)
                ?? throw new DomainException("One of the selected disability reasons does not exist.");
        }
    }

    private async Task EnsureChronicDiseasesExistAsync(List<Guid> ids, CancellationToken ct)
    {
        foreach (var id in ids)
        {
            _ = await chronicDiseases.GetByIdAsync(id, ct)
                ?? throw new DomainException("One of the selected chronic diseases does not exist.");
        }
    }

    private async Task SyncDisabilitiesAsync(Guid beneficiaryId,
        IReadOnlyList<SaveBeneficiaryDisabilityItem> requested, CancellationToken ct)
    {
        var current = await profiles.GetDisabilitiesAsync(beneficiaryId, ct);
        var requestedByType = requested.ToDictionary(i => i.DisabilityTypeId);

        var rowsToRemove = current.Where(x => !requestedByType.ContainsKey(x.DisabilityTypeId)).ToList();
        if (rowsToRemove.Count > 0)
            profiles.RemoveDisabilities(rowsToRemove);

        foreach (var row in current)
        {
            if (requestedByType.TryGetValue(row.DisabilityTypeId, out var item) && row.ReasonId != item.ReasonId)
                row.Update(item.ReasonId);   // changed reason updates the row; type never changes
        }

        var currentByType = current.Select(x => x.DisabilityTypeId).ToHashSet();
        foreach (var item in requested.Where(i => !currentByType.Contains(i.DisabilityTypeId)))
            await profiles.AddAsync(new BeneficiaryDisabilityEntity(beneficiaryId, item.DisabilityTypeId, item.ReasonId), ct);
    }

    private async Task SyncChronicDiseasesAsync(Guid beneficiaryId, List<Guid> requested, CancellationToken ct)
    {
        var current = await profiles.GetChronicDiseasesAsync(beneficiaryId, ct);
        var requestedIds = requested.ToHashSet();

        var rowsToRemove = current.Where(x => !requestedIds.Contains(x.ChronicDiseaseId)).ToList();
        if (rowsToRemove.Count > 0)
            profiles.RemoveChronicDiseases(rowsToRemove);

        var currentIds = current.Select(x => x.ChronicDiseaseId).ToHashSet();
        foreach (var id in requestedIds.Where(id => !currentIds.Contains(id)))
            await profiles.AddAsync(new BeneficiaryChronicDiseaseEntity(beneficiaryId, id), ct);
    }
}
