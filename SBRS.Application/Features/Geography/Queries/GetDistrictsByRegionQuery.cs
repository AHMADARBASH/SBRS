using MediatR;
using SBRS.Application.Features.Geography.DTOs;
using SBRS.Application.Features.Geography.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Geography.Queries;

public record GetDistrictsByRegionQuery(Guid RegionId) : IRequest<List<DistrictDto>>;


public class GetDistrictsByRegionQueryHandler(IGeographyRepository geographyRepository) : IRequestHandler<GetDistrictsByRegionQuery, List<DistrictDto>>
{
    
    public async Task<List<DistrictDto>> Handle(GetDistrictsByRegionQuery request, CancellationToken cancellationToken)
    {
        var districts = await geographyRepository.GetDistrictsByRegionAsync(request.RegionId);
        return districts.Select(d => d.ToDto()).ToList();
    }
}