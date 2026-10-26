using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Models;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Mappings;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Queries;

public sealed record GetBeneficiariesQuery(int Page, Guid? FacilityId)
    : IRequest<PagedResult<BeneficiaryListItemDto>>;

public sealed class GetBeneficiariesQueryValidator : AbstractValidator<GetBeneficiariesQuery>
{
    public GetBeneficiariesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100_000);
    }
}

public sealed class GetBeneficiariesQueryHandler(
    IBeneficiaryRepository repository, IBeneficiaryIdentityRepository identityRepository, ICurrentUser currentUser)
    : IRequestHandler<GetBeneficiariesQuery, PagedResult<BeneficiaryListItemDto>>
{
    private const int PageSize = 30;

    public async Task<PagedResult<BeneficiaryListItemDto>> Handle(GetBeneficiariesQuery request,
        CancellationToken cancellationToken)
    {
        Guid facilityId;
        if (currentUser.IsSystemAdmin)
        {
            facilityId = request.FacilityId
                ?? throw new DomainException("facilityId is required for this role.");
        }
        else
        {
            // DataEntry and FacilityAdmin always see their own facility; any facilityId in the request is ignored.
            facilityId = currentUser.FacilityId
                ?? throw new ForbiddenException("No facility is assigned to this user.");
        }

        var (items, totalCount) = await repository.GetPagedByFacilityAsync(
            facilityId, request.Page, PageSize, cancellationToken);

        // one extra query for the whole page
        var identities = await identityRepository.GetPrimaryByBeneficiaryIdsAsync(
            items.Select(b => b.Id).ToList(), cancellationToken);

        return new PagedResult<BeneficiaryListItemDto>(
            items.Select(b => b.ToListItemDto(identities.RequireIdentity(b))).ToList(),
            request.Page, PageSize, totalCount);
    }
}
