using MediatR;
using SBRS.Application.Features.DisabilityReasons.DTOs;
using SBRS.Application.Features.DisabilityReasons.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.DisabilityReasons.Queries;

public sealed record GetDisabilityReasonsQuery : IRequest<IReadOnlyList<DisabilityReasonDto>>;

public sealed class GetDisabilityReasonsQueryHandler(IDisabilityReasonRepository reasons)
    : IRequestHandler<GetDisabilityReasonsQuery, IReadOnlyList<DisabilityReasonDto>>
{
    public async Task<IReadOnlyList<DisabilityReasonDto>> Handle(GetDisabilityReasonsQuery q,
        CancellationToken ct) => (await reasons.GetAllAsync(ct)).Select(r => r.ToDto()).ToList();
}
