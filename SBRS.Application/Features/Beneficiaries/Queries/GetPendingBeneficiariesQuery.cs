using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Mappings;
using SBRS.Domain.Enums;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Queries;

public sealed record GetPendingBeneficiariesQuery : IRequest<IReadOnlyList<PendingBeneficiaryDto>>;

public sealed class GetPendingBeneficiariesQueryHandler(IBeneficiaryRepository repository, ICurrentUser currentUser)
    : IRequestHandler<GetPendingBeneficiariesQuery, IReadOnlyList<PendingBeneficiaryDto>>
{
    public async Task<IReadOnlyList<PendingBeneficiaryDto>> Handle(GetPendingBeneficiariesQuery request,
        CancellationToken cancellationToken)
    {
        Guid? facilityId = currentUser.IsSystemAdmin
            ? null
            : currentUser.FacilityId ?? throw new ForbiddenException("No facility is assigned to this user.");

        var pending = await repository.GetPendingReviewAsync(facilityId, cancellationToken);
        return pending.Select(b => b.ToPendingDto()).ToList();
    }
}
