using MediatR;
using SBRS.Application.Features.Nationalities.DTOs;
using SBRS.Application.Features.Nationalities.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Nationalities.Queries;

public sealed record GetNationalitiesQuery : IRequest<IReadOnlyList<NationalityDto>>;

public sealed class GetNationalitiesQueryHandler(INationalityRepository nationalities)
    : IRequestHandler<GetNationalitiesQuery, IReadOnlyList<NationalityDto>>
{
    public async Task<IReadOnlyList<NationalityDto>> Handle(GetNationalitiesQuery q, CancellationToken ct) =>
        (await nationalities.GetAllAsync(ct)).Select(n => n.ToDto()).ToList();
}
