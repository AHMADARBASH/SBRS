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
    DateOnly EnrollmentDate) : IRequest<RegisterBeneficiaryResultDto>;

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
    }
}

public sealed class RegisterBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaries,
    IFacilityRepository facilities,
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

        var code = await beneficiaries.GetNextCodeAsync(cancellationToken);
        var fullCode = $"{facility.Code}-{code}";   // FullCode = FacilityCode-Code, no zero padding

        var beneficiary = new BeneficiaryEntity(
            request.FirstName, request.FatherName, request.LastName, request.MotherName,
            birthDate, request.IsBirthDateEstimated, request.Gender, request.EnrollmentDate,
            code, fullCode, facilityId, disambiguator, reviewStatus);

        await beneficiaries.AddAsync(beneficiary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterBeneficiaryResultDto(beneficiary.Id, code, fullCode, reviewStatus);
    }
}
