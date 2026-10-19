using MediatR;
using SBRS.Application.Features.FacilityTypes.DTOs;
using SBRS.Application.Features.FacilityTypes.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.FacilityTypes.Queries;

public sealed record GetFacilityTypesQuery(bool IncludeInactive) : IRequest<IReadOnlyList<FacilityTypeDto>>;

public sealed class GetFacilityTypesQueryHandler(IFacilityTypeRepository types)
    : IRequestHandler<GetFacilityTypesQuery, IReadOnlyList<FacilityTypeDto>>
{
    public async Task<IReadOnlyList<FacilityTypeDto>> Handle(GetFacilityTypesQuery q, CancellationToken ct) =>
        (await types.GetAllAsync(ct)).Select(t => t.ToDto()).ToList();
}