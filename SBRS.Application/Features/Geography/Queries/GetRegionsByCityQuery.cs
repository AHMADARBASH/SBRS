using MediatR;
using SBRS.Application.Features.Geography.DTOs;
using SBRS.Application.Features.Geography.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Geography.Queries;

public record GetRegionsByCityQuery(Guid CityId) : IRequest<List<RegionDto>>;

public class GetRegionsByCityIdQueryHandler(IGeographyRepository geographyRepository):IRequestHandler<GetRegionsByCityQuery, List<RegionDto>>
{

    public async Task<List<RegionDto>> Handle(GetRegionsByCityQuery request, CancellationToken ct)
        => (await geographyRepository.GetRegionsByCityAsync(request.CityId, ct)).Select(r => r.ToDto()).ToList();
}