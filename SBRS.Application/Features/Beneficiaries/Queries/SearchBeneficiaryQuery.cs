using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Mappings;
using SBRS.Application.Features.Beneficiaries.Services;
using SBRS.Domain.Common;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Queries;

public sealed record SearchBeneficiaryQuery(
    string? FullCode,
    string? FirstName,
    string? FatherName,
    string? LastName,
    string? MotherName,
    DateOnly? BirthDate) : IRequest<BeneficiarySearchResultDto>, IBeneficiarySearchKeys;

public sealed class SearchBeneficiaryQueryValidator : BeneficiarySearchKeysValidator<SearchBeneficiaryQuery> { }

public sealed class SearchBeneficiaryQueryHandler(
    IBeneficiaryRepository repository,
    IBeneficiaryIdentityRepository identityRepository,
    IFacilityBeneficiaryRepository enrollments,
    ICurrentUser currentUser)
    : IRequestHandler<SearchBeneficiaryQuery, BeneficiarySearchResultDto>
{
    public async Task<BeneficiarySearchResultDto> Handle(SearchBeneficiaryQuery request,
        CancellationToken cancellationToken)
    {
        // The validator guarantees at least one key before this handler runs. Only the
        // normalized values go to the repository: names are never logged at Information level.
        var criteria = BeneficiarySearchHelper.BuildCriteria(request);

        var matches = await repository.SearchAsync(criteria, BeneficiarySearchHelper.MaxResults + 1,
            cancellationToken);

        // one extra query for the whole page; the search shows the identity type but never the value
        var identities = await identityRepository.GetPrimaryByBeneficiaryIdsAsync(
            matches.Select(b => b.Id).ToList(), cancellationToken);

        // one batched enrollment lookup; SystemAdmin has no facility claim, so the flag is false
        var enrolledIds = currentUser.FacilityId is { } facilityId
            ? await enrollments.GetEnrolledBeneficiaryIdsAsync(
                matches.Select(b => b.Id).ToList(), facilityId, cancellationToken)
            : new HashSet<Guid>();

        return BeneficiarySearchHelper.ToResult(matches, identities, enrolledIds);
    }
}
