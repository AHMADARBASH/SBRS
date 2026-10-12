using MediatR;
using SBRS.Application.Features.Geography.DTOs;
using SBRS.Application.Features.Geography.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Geography.Queries;


public record GetCountriesQuery() : IRequest<List<CountryDto>>;

public class GetCountriesQueryHandler(IGeographyRepository geographyRepository)
    : IRequestHandler<GetCountriesQuery, List<CountryDto>>
{
    public async Task<List<CountryDto>> Handle(GetCountriesQuery request, CancellationToken ct)
        => (await geographyRepository.GetCountriesAsync(ct))
            .Select(x => x.ToDto()).ToList();
}

