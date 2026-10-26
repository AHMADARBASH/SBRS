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
    IBeneficiaryRepository beneficiaries, IBeneficiaryIdentityRepository identities, ICurrentUser currentUser)
    : IRequestHandler<GetBeneficiaryIdentitiesQuery, IReadOnlyList<BeneficiaryIdentityDto>>
{
    public async Task<IReadOnlyList<BeneficiaryIdentityDto>> Handle(GetBeneficiaryIdentitiesQuery request,
        CancellationToken cancellationToken)
    {
        var beneficiary = await beneficiaries.GetByIdAsync(request.BeneficiaryId, cancellationToken);

        // Out-of-scope records look like missing ones (same rule as the other features).
        if (beneficiary is null ||
            (!currentUser.IsSystemAdmin && beneficiary.RegistrationFacilityId != currentUser.FacilityId))
            throw new NotFoundException(nameof(BeneficiaryEntity), request.BeneficiaryId);

        var list = await identities.GetByBeneficiaryIdAsync(beneficiary.Id, cancellationToken);
        return list.Select(i => i.ToDto()).ToList();
    }
}
