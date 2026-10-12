using MediatR;
using SBRS.Application.Features.Geography.DTOs;
using SBRS.Application.Features.Geography.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Geography.Queries;


public record GetAreasByDistrictQuery(Guid DistrictId) : IRequest<List<AreaDto>>;


public class GetAreasByDistrictQueryHandler(IGeographyRepository geographyRepository) : IRequestHandler<GetAreasByDistrictQuery, List<AreaDto>>
{
    public async Task<List<AreaDto>> Handle(GetAreasByDistrictQuery request, CancellationToken cancellationToken)
    {
        var areas = await geographyRepository.GetAreasByDistrictAsync(request.DistrictId);
        return areas.Select(a => a.ToDto()).ToList();
    }
}