using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Services;
using SBRS.Domain.Constants;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Queries;

/// <summary>Same search as SearchBeneficiaryQuery, limited to the active roster of one facility.</summary>
public sealed record SearchEnrolledBeneficiaryQuery(
    Guid FacilityId,
    string? FullCode,
    string? FirstName,
    string? FatherName,
    string? LastName,
    string? MotherName,
    DateOnly? BirthDate) : IRequest<BeneficiarySearchResultDto>, IBeneficiarySearchKeys;

public sealed class SearchEnrolledBeneficiaryQueryValidator
    : BeneficiarySearchKeysValidator<SearchEnrolledBeneficiaryQuery>
{
    public SearchEnrolledBeneficiaryQueryValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty();
    }
}

public sealed class SearchEnrolledBeneficiaryQueryHandler(
    IEnrolledBeneficiarySearchRepository enrolledSearch,
    IBeneficiaryIdentityRepository identityRepository,
    ICurrentUser currentUser) : IRequestHandler<SearchEnrolledBeneficiaryQuery, BeneficiarySearchResultDto>
{
    public async Task<BeneficiarySearchResultDto> Handle(SearchEnrolledBeneficiaryQuery request,
        CancellationToken cancellationToken)
    {
        var facilityId = ResolveFacilityId(request, currentUser);
        var criteria = BeneficiarySearchHelper.BuildCriteria(request);

        var matches = await enrolledSearch.SearchEnrolledAsync(
            criteria, facilityId, BeneficiarySearchHelper.MaxResults + 1, cancellationToken);

        var identities = await identityRepository.GetPrimaryByBeneficiaryIdsAsync(
            matches.Select(b => b.Id).ToList(), cancellationToken);

        // Every match is enrolled at the target facility by the database filter.
        var enrolledIds = matches.Select(b => b.Id).ToHashSet();
        return BeneficiarySearchHelper.ToResult(matches, identities, enrolledIds);
    }

    private static Guid ResolveFacilityId(SearchEnrolledBeneficiaryQuery request, ICurrentUser currentUser)
    {
        if (currentUser.IsSystemAdmin)
            return request.FacilityId;

        if (currentUser.Role is Roles.DataEntry or Roles.FacilityAdmin)
            return currentUser.FacilityId
                   ?? throw new DomainException("No facility is assigned to this user.");

        throw new DomainException("This role cannot search enrolled beneficiaries.");
    }
}
