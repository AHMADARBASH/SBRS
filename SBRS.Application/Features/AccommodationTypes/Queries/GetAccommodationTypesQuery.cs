using MediatR;
using SBRS.Application.Features.AccommodationTypes.DTOs;
using SBRS.Application.Features.AccommodationTypes.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.AccommodationTypes.Queries;

public sealed record GetAccommodationTypesQuery : IRequest<IReadOnlyList<AccommodationTypeDto>>;

public sealed class GetAccommodationTypesQueryHandler(IAccommodationTypeRepository accommodationTypes)
    : IRequestHandler<GetAccommodationTypesQuery, IReadOnlyList<AccommodationTypeDto>>
{
    public async Task<IReadOnlyList<AccommodationTypeDto>> Handle(GetAccommodationTypesQuery q, CancellationToken ct) =>
        (await accommodationTypes.GetAllAsync(ct)).Select(a => a.ToDto()).ToList();
}
