using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Queries;

public sealed record GetBeneficiaryIdentitiesQuery(Guid BeneficiaryId) : IRequest<IReadOnlyList<BeneficiaryIdentityDto>>;

public sealed class GetBeneficiaryIdentitiesQueryHandler(
    IBeneficiaryRepository beneficiaries, IBeneficiaryIdentityRepository identities,
    IFacilityBeneficiaryRepository enrollments, ICurrentUser currentUser)
    : IRequestHandler<GetBeneficiaryIdentitiesQuery, IReadOnlyList<BeneficiaryIdentityDto>>
{
    public async Task<IReadOnlyList<BeneficiaryIdentityDto>> Handle(GetBeneficiaryIdentitiesQuery request,
        CancellationToken cancellationToken)
    {
        var beneficiary = await beneficiaries.GetByIdAsync(request.BeneficiaryId, cancellationToken);

        // Scope follows the facility roster: an enrolled facility may read the identities even
        // when the person was first registered elsewhere. Out-of-scope looks like missing (ADR-006).
        var isInScope = currentUser.IsSystemAdmin ||
                        (currentUser.FacilityId is { } facilityId &&
                         await enrollments.IsEnrolledAsync(request.BeneficiaryId, facilityId, cancellationToken));

        if (beneficiary is null || !isInScope)
            throw new NotFoundException(nameof(BeneficiaryEntity), request.BeneficiaryId);

        var list = await identities.GetByBeneficiaryIdAsync(beneficiary.Id, cancellationToken);
        return list.Select(i => i.ToDto()).ToList();
    }
}
