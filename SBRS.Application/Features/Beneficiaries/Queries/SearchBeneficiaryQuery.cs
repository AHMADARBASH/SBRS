using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Mappings;
using SBRS.Domain.Common;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Queries;

public sealed record SearchBeneficiaryQuery(
    string? FullCode,
    string? FirstName,
    string? FatherName,
    string? LastName,
    string? MotherName,
    DateOnly? BirthDate) : IRequest<BeneficiarySearchResultDto>;

public sealed class SearchBeneficiaryQueryValidator : AbstractValidator<SearchBeneficiaryQuery>
{
    private const int MinNameLength = 2;

    public SearchBeneficiaryQueryValidator()
    {
        RuleFor(x => x.FullCode).MaximumLength(50);

        // A name that normalizes to nothing is "not provided" (see the group rule below).
        // A provided name must still reach the minimum length after normalization.
        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .Must(ReachesMinimumLength).When(x => NormalizedLength(x.FirstName) > 0)
            .WithMessage("First name must be at least 2 characters after normalization.");
        RuleFor(x => x.FatherName)
            .MaximumLength(100)
            .Must(ReachesMinimumLength).When(x => NormalizedLength(x.FatherName) > 0)
            .WithMessage("Father name must be at least 2 characters after normalization.");
        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .Must(ReachesMinimumLength).When(x => NormalizedLength(x.LastName) > 0)
            .WithMessage("Last name must be at least 2 characters after normalization.");
        RuleFor(x => x.MotherName)
            .MaximumLength(100)
            .Must(ReachesMinimumLength).When(x => NormalizedLength(x.MotherName) > 0)
            .WithMessage("Mother name must be at least 2 characters after normalization.");

        // null, empty, whitespace and names that normalize to nothing all count as not provided.
        RuleFor(x => x)
            .Must(HasAtLeastOneKey)
            .WithMessage("At least one search key is required.");
    }

    private static int NormalizedLength(string? value) =>
        ArabicTextNormalizer.Normalize(value).Length;

    private static bool ReachesMinimumLength(string? value) =>
        NormalizedLength(value) >= MinNameLength;

    private static bool HasAtLeastOneKey(SearchBeneficiaryQuery x) =>
        !string.IsNullOrWhiteSpace(x.FullCode) ||
        NormalizedLength(x.FirstName) > 0 ||
        NormalizedLength(x.FatherName) > 0 ||
        NormalizedLength(x.LastName) > 0 ||
        NormalizedLength(x.MotherName) > 0 ||
        x.BirthDate is not null;
}

public sealed class SearchBeneficiaryQueryHandler(
    IBeneficiaryRepository repository,
    IBeneficiaryIdentityRepository identityRepository,
    IFacilityBeneficiaryRepository enrollments,
    ICurrentUser currentUser)
    : IRequestHandler<SearchBeneficiaryQuery, BeneficiarySearchResultDto>
{
    private const int MaxResults = 20;

    public async Task<BeneficiarySearchResultDto> Handle(SearchBeneficiaryQuery request,
        CancellationToken cancellationToken)
    {
        // The validator guarantees at least one key before this handler runs. Only the
        // normalized values go to the repository: names are never logged at Information level.
        var criteria = new BeneficiarySearchCriteria(
            NormalizeFullCode(request.FullCode),
            NormalizeName(request.FirstName),
            NormalizeName(request.FatherName),
            NormalizeName(request.LastName),
            NormalizeName(request.MotherName),
            request.BirthDate);

        var matches = await repository.SearchAsync(criteria, MaxResults + 1, cancellationToken);

        // one extra query for the whole page; the search shows the identity type but never the value
        var identities = await identityRepository.GetPrimaryByBeneficiaryIdsAsync(
            matches.Select(b => b.Id).ToList(), cancellationToken);

        // one batched enrollment lookup; SystemAdmin has no facility claim, so the flag is false
        var enrolledIds = currentUser.FacilityId is { } facilityId
            ? await enrollments.GetEnrolledBeneficiaryIdsAsync(
                matches.Select(b => b.Id).ToList(), facilityId, cancellationToken)
            : new HashSet<Guid>();

        return new BeneficiarySearchResultDto(
            matches.Take(MaxResults)
                .Select(b => b.ToSearchItemDto(identities.RequireIdentity(b), enrolledIds.Contains(b.Id)))
                .ToList(),
            matches.Count > MaxResults);
    }

    // Generated FullCodes are upper case; citext makes the database comparison case-insensitive too.
    private static string? NormalizeFullCode(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static string? NormalizeName(string? value)
    {
        var normalized = ArabicTextNormalizer.Normalize(value);
        return normalized.Length == 0 ? null : normalized;
    }
}
