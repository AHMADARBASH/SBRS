using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Services;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Commands;

public sealed record UpdateBeneficiaryRegistrationCommand(
    Guid Id,
    string FirstName,
    string FatherName,
    string LastName,
    string MotherName,
    DateOnly BirthDate,
    bool IsBirthDateEstimated,
    bool Gender,
    Guid AreaId,
    Guid NationalityId,
    Guid IdentityTypeId,
    string? IdentityValue) : IRequest<RegisterBeneficiaryResultDto>;

public sealed class UpdateBeneficiaryRegistrationCommandValidator
    : AbstractValidator<UpdateBeneficiaryRegistrationCommand>
{
    public UpdateBeneficiaryRegistrationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100)
            .Must(v => ArabicTextNormalizer.Normalize(v).Length > 0);
        RuleFor(x => x.FatherName).NotEmpty().MaximumLength(100)
            .Must(v => ArabicTextNormalizer.Normalize(v).Length > 0);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100)
            .Must(v => ArabicTextNormalizer.Normalize(v).Length > 0);
        RuleFor(x => x.MotherName).NotEmpty().MaximumLength(100)
            .Must(v => ArabicTextNormalizer.Normalize(v).Length > 0);

        RuleFor(x => x).Must(x =>
        {
            var resolved = BeneficiaryBirthDate.Resolve(x.BirthDate, x.IsBirthDateEstimated);
            return resolved.Year >= 1900 && resolved <= DateOnly.FromDateTime(DateTime.UtcNow);
        }).WithMessage("Birth date is invalid.");

        RuleFor(x => x.AreaId).NotEmpty();
        RuleFor(x => x.NationalityId).NotEmpty();

        RuleFor(x => x.IdentityTypeId).NotEmpty();
        RuleFor(x => x.IdentityValue).MaximumLength(30);   // the regex of the type does the real validation
    }
}

public sealed class UpdateBeneficiaryRegistrationCommandHandler(
    IBeneficiaryRepository beneficiaries,
    IFacilityBeneficiaryRepository enrollments,
    IGeographyRepository geography,
    IIdentityTypeRepository identityTypes,
    INationalityRepository nationalities,
    IBeneficiaryIdentityRepository identities,
    IBeneficiaryMatchService matchService,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBeneficiaryRegistrationCommand, RegisterBeneficiaryResultDto>
{
    public async Task<RegisterBeneficiaryResultDto> Handle(UpdateBeneficiaryRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        // DataEntry edits records of their own facility only: the roster (active enrollment) decides,
        // exactly like the facility GetAll list.
        var facilityId = currentUser.FacilityId
            ?? throw new ForbiddenException("No facility is assigned to this user.");

        var beneficiary = await beneficiaries.GetByIdAsync(request.Id, cancellationToken);

        // Missing, merged duplicates and deactivated rows are out of scope and look like missing ones.
        if (beneficiary is null || beneficiary.MergedIntoBeneficiaryId is not null || !beneficiary.IsActive)
            throw new NotFoundException(nameof(BeneficiaryEntity), request.Id);

        if (!await enrollments.IsEnrolledAsync(beneficiary.Id, facilityId, cancellationToken))
            throw new NotFoundException(nameof(BeneficiaryEntity), request.Id);

        // The area must exist (it carries the city implicitly); the request never contains the city.
        _ = await geography.GetAreaByIdAsync(request.AreaId, cancellationToken)
            ?? throw new DomainException("The selected area does not exist.");

        _ = await nationalities.GetByIdAsync(request.NationalityId, cancellationToken)
            ?? throw new DomainException("The selected nationality does not exist.");

        var identityType = await identityTypes.GetByIdAsync(request.IdentityTypeId, cancellationToken)
            ?? throw new DomainException("The selected identity type does not exist.");

        // Identity values are normalized (trim + digit conversion) and never logged.
        var identityValue = IdentityValueNormalizer.Normalize(request.IdentityValue);
        if (!identityType.IsValidValue(identityValue))
            throw new DomainException($"The identity value is not valid for the identity type {identityType.Code}.");

        var birthDate = BeneficiaryBirthDate.Resolve(request.BirthDate, request.IsBirthDateEstimated);
        var key = BeneficiaryIdentityKey.From(request.FirstName, request.FatherName, request.LastName,
            request.MotherName, birthDate);

        // Only a change of the five normalized matching fields can change the match data.
        var keyChanged =
            !string.Equals(key.FirstName, beneficiary.NormalizedFirstName, StringComparison.Ordinal) ||
            !string.Equals(key.FatherName, beneficiary.NormalizedFatherName, StringComparison.Ordinal) ||
            !string.Equals(key.LastName, beneficiary.NormalizedLastName, StringComparison.Ordinal) ||
            !string.Equals(key.MotherName, beneficiary.NormalizedMotherName, StringComparison.Ordinal) ||
            key.BirthDate != beneficiary.BirthDate;

        BeneficiaryMatchReviewStatus? reviewStatus = null;
        var disambiguator = 0;

        if (keyChanged)
        {
            // Same mechanism as registration: document match, exact key, then probable candidates.
            var match = await matchService.FindAsync(key, request.IsBirthDateEstimated, beneficiary.Id,
                cancellationToken, new[] { (identityType.Id, identityValue) });

            reviewStatus = match.Status == BeneficiaryMatchStatus.NotFound
                ? BeneficiaryMatchReviewStatus.NoMatchFound
                : BeneficiaryMatchReviewStatus.PendingReview;

            if (match.Status == BeneficiaryMatchStatus.Exact)
                disambiguator = await beneficiaries.GetNextDisambiguatorAsync(key.MatchKey, cancellationToken);
        }

        beneficiary.Update(request.FirstName, request.FatherName, request.LastName, request.MotherName,
            request.BirthDate, request.IsBirthDateEstimated, request.Gender, request.AreaId, request.NationalityId);

        if (reviewStatus is { } status)
            beneficiary.SetMatchReview(status, disambiguator);

        // The primary identity (the oldest row) is rewritten in place; a beneficiary without any
        // identity row gets one. One identity per type per beneficiary is kept by construction.
        var primaryIdentity = await identities.GetPrimaryForUpdateAsync(beneficiary.Id, cancellationToken);
        if (primaryIdentity is null)
            await identities.AddAsync(
                new BeneficiaryIdentityEntity(beneficiary.Id, identityType, identityValue), cancellationToken);
        else
            primaryIdentity.Update(identityType, identityValue);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterBeneficiaryResultDto(
            beneficiary.Id, beneficiary.Code, beneficiary.FullCode, beneficiary.MatchReviewStatus);
    }
}
