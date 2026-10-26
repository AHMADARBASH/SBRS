using FluentValidation;
using MediatR;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Mappings;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Queries;

public sealed record SearchBeneficiaryQuery(string? FullCode, string? FirstName, string? LastName)
    : IRequest<BeneficiarySearchResultDto>;

public sealed class SearchBeneficiaryQueryValidator : AbstractValidator<SearchBeneficiaryQuery>
{
    public SearchBeneficiaryQueryValidator()
    {
        RuleFor(x => x.FullCode).MaximumLength(50);
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);

        // Either the FullCode, or both names with at least 2 meaningful characters each.
        RuleFor(x => x).Must(x =>
                !string.IsNullOrWhiteSpace(x.FullCode) ||
                (HasName(x.FirstName) && HasName(x.LastName)))
            .WithMessage("Provide the full code, or both first name and last name (at least 2 characters each).");
    }

    private static bool HasName(string? value) => ArabicTextNormalizer.Normalize(value).Length >= 2;
}

public sealed class SearchBeneficiaryQueryHandler(IBeneficiaryRepository repository,
    IBeneficiaryIdentityRepository identityRepository)
    : IRequestHandler<SearchBeneficiaryQuery, BeneficiarySearchResultDto>
{
    private const int MaxResults = 20;

    public async Task<BeneficiarySearchResultDto> Handle(SearchBeneficiaryQuery request,
        CancellationToken cancellationToken)
    {
        BeneficiaryEntity? found = null;

        if (!string.IsNullOrWhiteSpace(request.FullCode))
        {
            found = await repository.GetByFullCodeAsync(request.FullCode.Trim(), cancellationToken);

            // A merged duplicate leads to the person it was merged into.
            if (found is { MatchReviewStatus: BeneficiaryMatchReviewStatus.MergedIntoExisting } &&
                found.MergedIntoBeneficiaryId is { } survivorId)
            {
                found = await repository.GetByIdAsync(survivorId, cancellationToken);
            }

            if (found is null || found.MatchReviewStatus == BeneficiaryMatchReviewStatus.MergedIntoExisting)
                return new BeneficiarySearchResultDto(Array.Empty<BeneficiarySearchItemDto>(), false);

            var identity = await identityRepository.GetPrimaryByBeneficiaryIdsAsync(new[] { found.Id }, cancellationToken);
            return new BeneficiarySearchResultDto(new[] { found.ToSearchItemDto(identity.RequireIdentity(found)) }, false);
        }

        var firstName = ArabicTextNormalizer.Normalize(request.FirstName);
        var lastName = ArabicTextNormalizer.Normalize(request.LastName);

        var matches = await repository.SearchByNameAsync(firstName, lastName, MaxResults + 1, cancellationToken);

        // one extra query for the whole page; the search shows the identity type but never the value
        var identities = await identityRepository.GetPrimaryByBeneficiaryIdsAsync(
            matches.Select(b => b.Id).ToList(), cancellationToken);

        return new BeneficiarySearchResultDto(
            matches.Take(MaxResults).Select(b => b.ToSearchItemDto(identities.RequireIdentity(b))).ToList(),
            matches.Count > MaxResults);
    }
}
