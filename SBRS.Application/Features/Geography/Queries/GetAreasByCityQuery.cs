using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Geography.DTOs;
using SBRS.Application.Features.Geography.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Geography.Queries;

public record GetAreasByCityQuery(Guid CityId) : IRequest<List<AreaDto>>;

public class GetAreasByCityQueryHandler(IGeographyRepository geographyRepository)
    : IRequestHandler<GetAreasByCityQuery, List<AreaDto>>
{
    public async Task<List<AreaDto>> Handle(GetAreasByCityQuery request, CancellationToken cancellationToken)
    {
        _ = await geographyRepository.GetCityByIdAsync(request.CityId, cancellationToken)
            ?? throw new NotFoundException(nameof(CityEntity), request.CityId);

        var areas = await geographyRepository.GetAreasByCityAsync(request.CityId, cancellationToken);
        return areas.Select(a => a.ToDto()).ToList();
    }
}
