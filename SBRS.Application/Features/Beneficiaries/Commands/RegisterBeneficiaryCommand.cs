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

public sealed record RegisterBeneficiaryCommand(
    string FirstName,
    string FatherName,
    string LastName,
    string MotherName,
    DateOnly BirthDate,
    bool IsBirthDateEstimated,
    bool Gender,
    DateOnly EnrollmentDate,
    Guid AreaId,
    Guid NationalityId,
    Guid IdentityTypeId,
    string? IdentityValue) : IRequest<RegisterBeneficiaryResultDto>;

public sealed class RegisterBeneficiaryCommandValidator : AbstractValidator<RegisterBeneficiaryCommand>
{
    public RegisterBeneficiaryCommandValidator()
    {
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

        RuleFor(x => x.EnrollmentDate)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("Enrollment date cannot be in the future.");

        RuleFor(x => x.AreaId).NotEmpty();
        RuleFor(x => x.NationalityId).NotEmpty();

        RuleFor(x => x.IdentityTypeId).NotEmpty();
        RuleFor(x => x.IdentityValue).MaximumLength(30);   // the regex of the type does the real validation
    }
}

public sealed class RegisterBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaries,
    IFacilityRepository facilities,
    IGeographyRepository geography,
    IIdentityTypeRepository identityTypes,
    INationalityRepository nationalities,
    IBeneficiaryIdentityRepository identities,
    IBeneficiaryMatchService matchService,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterBeneficiaryCommand, RegisterBeneficiaryResultDto>
{
    public async Task<RegisterBeneficiaryResultDto> Handle(RegisterBeneficiaryCommand request,
        CancellationToken cancellationToken)
    {
        // Data entry registers at their own facility only.
        var facilityId = currentUser.FacilityId
            ?? throw new ForbiddenException("No facility is assigned to this user.");

        var facility = await facilities.GetByIdAsync(facilityId, cancellationToken)
            ?? throw new NotFoundException(nameof(FacilityEntity), facilityId);
        if (!facility.IsActive) throw new DomainException("The facility is not active.");

        // The area must exist. It carries the city implicitly; the request never contains the city.
        var area = await geography.GetAreaByIdAsync(request.AreaId, cancellationToken)
            ?? throw new DomainException("The selected area does not exist.");

        // The nationality must exist; it is stored on the beneficiary itself, not on the (future) profile.
        _ = await nationalities.GetByIdAsync(request.NationalityId, cancellationToken)
            ?? throw new DomainException("The selected nationality does not exist.");

        // The identity is validated early so the user gets the error before scoring anything.
        var identityType = await identityTypes.GetByIdAsync(request.IdentityTypeId, cancellationToken)
            ?? throw new DomainException("The selected identity type does not exist.");

        var identityValue = IdentityValueNormalizer.Normalize(request.IdentityValue);
        if (!identityType.IsValidValue(identityValue))
            throw new DomainException($"The identity value is not valid for the identity type {identityType.Code}.");

        var birthDate = BeneficiaryBirthDate.Resolve(request.BirthDate, request.IsBirthDateEstimated);
        var key = BeneficiaryIdentityKey.From(request.FirstName, request.FatherName, request.LastName,
            request.MotherName, birthDate);

        var match = await matchService.FindAsync(key, request.IsBirthDateEstimated, null, cancellationToken);

        var reviewStatus = match.Status == BeneficiaryMatchStatus.NotFound
            ? BeneficiaryMatchReviewStatus.NoMatchFound
            : BeneficiaryMatchReviewStatus.PendingReview;

        var disambiguator = match.Status == BeneficiaryMatchStatus.Exact
            ? await beneficiaries.GetNextDisambiguatorAsync(key.MatchKey, cancellationToken)
            : 0;

        // ---- everything above can reject the registration without consuming a number ----
        // ---- below, a short locked transaction assigns the per-facility code ----
        // Registrations in the same facility run one at a time: the facility row lock serializes
        // them, then next = max Code (including deactivated rows) + 1. A number is only consumed
        // when the beneficiary and its identity are really saved; any failure rolls everything
        // back, releasing the number.
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await beneficiaries.LockRegistrationFacilityAsync(facilityId, cancellationToken);

            var code = await beneficiaries.GetMaxCodeForFacilityAsync(facilityId, cancellationToken) + 1;
            var fullCode = BeneficiaryFullCodeBuilder.Build(facility.Code, code);

            var beneficiary = new BeneficiaryEntity(
                request.FirstName, request.FatherName, request.LastName, request.MotherName,
                birthDate, request.IsBirthDateEstimated, request.Gender, request.EnrollmentDate,
                request.AreaId, request.NationalityId,
                code, fullCode, facilityId, disambiguator, reviewStatus);

            await beneficiaries.AddAsync(beneficiary, cancellationToken);

            // saved in the same SaveChangesAsync call as the beneficiary (the id is assigned on AddAsync)
            var identity = new BeneficiaryIdentityEntity(beneficiary.Id, identityType, identityValue);
            await identities.AddAsync(identity, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return new RegisterBeneficiaryResultDto(beneficiary.Id, code, fullCode, reviewStatus);
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
