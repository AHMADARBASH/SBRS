using MediatR;
using SBRS.Application.Features.DisabilityTypes.DTOs;
using SBRS.Application.Features.DisabilityTypes.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.DisabilityTypes.Queries;

public sealed record GetDisabilityTypesQuery : IRequest<IReadOnlyList<DisabilityTypeDto>>;

public sealed class GetDisabilityTypesQueryHandler(IDisabilityTypeRepository disabilityTypes)
    : IRequestHandler<GetDisabilityTypesQuery, IReadOnlyList<DisabilityTypeDto>>
{
    public async Task<IReadOnlyList<DisabilityTypeDto>> Handle(GetDisabilityTypesQuery q, CancellationToken ct) =>
        (await disabilityTypes.GetAllAsync(ct)).Select(d => d.ToDto()).ToList();
}
