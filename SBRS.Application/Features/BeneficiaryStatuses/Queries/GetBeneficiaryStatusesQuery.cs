using MediatR;
using SBRS.Application.Features.BeneficiaryStatuses.DTOs;
using SBRS.Application.Features.BeneficiaryStatuses.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryStatuses.Queries;

public sealed record GetBeneficiaryStatusesQuery : IRequest<IReadOnlyList<BeneficiaryStatusDto>>;

public sealed class GetBeneficiaryStatusesQueryHandler(IBeneficiaryStatusRepository statuses)
    : IRequestHandler<GetBeneficiaryStatusesQuery, IReadOnlyList<BeneficiaryStatusDto>>
{
    public async Task<IReadOnlyList<BeneficiaryStatusDto>> Handle(GetBeneficiaryStatusesQuery q,
        CancellationToken ct) => (await statuses.GetAllAsync(ct)).Select(s => s.ToDto()).ToList();
}
