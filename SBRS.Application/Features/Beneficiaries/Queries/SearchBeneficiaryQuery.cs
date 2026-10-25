using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Mappings;
using SBRS.Application.Features.Beneficiaries.Services;
using SBRS.Domain.Common;
using SBRS.Domain.Constants;

namespace SBRS.Application.Features.Beneficiaries.Queries;

public sealed record SearchBeneficiaryQuery(
    string FirstName,
    string FatherName,
    string LastName,
    string MotherName,
    DateOnly BirthDate,
    bool IsBirthDateEstimated) : IRequest<BeneficiarySearchResultDto>;

public sealed class SearchBeneficiaryQueryValidator : AbstractValidator<SearchBeneficiaryQuery>
{
    public SearchBeneficiaryQueryValidator()
    {
        // Privacy rule: the cross-facility search needs all five identity fields.
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100)
            .Must(n => ArabicTextNormalizer.Normalize(n) != string.Empty)
            .WithMessage("First name must contain at least one meaningful character.");

        RuleFor(x => x.FatherName)
            .NotEmpty().WithMessage("Father name is required.")
            .MaximumLength(100)
            .Must(n => ArabicTextNormalizer.Normalize(n) != string.Empty)
            .WithMessage("Father name must contain at least one meaningful character.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100)
            .Must(n => ArabicTextNormalizer.Normalize(n) != string.Empty)
            .WithMessage("Last name must contain at least one meaningful character.");

        RuleFor(x => x.MotherName)
            .NotEmpty().WithMessage("Mother name is required.")
            .MaximumLength(100)
            .Must(n => ArabicTextNormalizer.Normalize(n) != string.Empty)
            .WithMessage("Mother name must contain at least one meaningful character.");

        RuleFor(x => x).Must(x =>
        {
            var resolved = BeneficiaryBirthDate.Resolve(x.BirthDate, x.IsBirthDateEstimated);
            return resolved.Year >= 1900 && resolved <= DateOnly.FromDateTime(DateTime.UtcNow);
        }).WithMessage("Birth date is invalid.");
    }
}

public sealed class SearchBeneficiaryQueryHandler(IBeneficiaryMatchService matchService, ICurrentUser currentUser)
    : IRequestHandler<SearchBeneficiaryQuery, BeneficiarySearchResultDto>
{
    public async Task<BeneficiarySearchResultDto> Handle(SearchBeneficiaryQuery q, CancellationToken ct)
    {
        var birthDate = BeneficiaryBirthDate.Resolve(q.BirthDate, q.IsBirthDateEstimated);
        var key = BeneficiaryIdentityKey.From(q.FirstName, q.FatherName, q.LastName, q.MotherName, birthDate);

        var result = await matchService.FindAsync(key, q.IsBirthDateEstimated, null, ct);

        // Data entry only learns whether a possible match exists; the facility admin handles the cases.
        var canSeeDetails = currentUser.Role != Roles.DataEntry;
        return result.ToDto(canSeeDetails);
    }
}
