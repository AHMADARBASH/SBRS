using MediatR;
using SBRS.Application.Features.Geography.DTOs;
using SBRS.Application.Features.Geography.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Geography.Queries;


public record GetCitiesByCountryQuery(Guid CountryId) : IRequest<List<CityDto>>;

public class GetCitiesByCountryQueryHandler(IGeographyRepository geographyRepository) : IRequestHandler<GetCitiesByCountryQuery, List<CityDto>>
{

    public async Task<List<CityDto>> Handle(GetCitiesByCountryQuery request, CancellationToken ct)

      => (await geographyRepository.GetCitiesByCountryAsync(request.CountryId, ct)).Select(c => c.ToDto()).ToList();

}