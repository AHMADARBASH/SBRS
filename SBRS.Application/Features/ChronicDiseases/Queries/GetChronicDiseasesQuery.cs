using MediatR;
using SBRS.Application.Features.ChronicDiseases.DTOs;
using SBRS.Application.Features.ChronicDiseases.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.ChronicDiseases.Queries;

public sealed record GetChronicDiseasesQuery : IRequest<IReadOnlyList<ChronicDiseaseDto>>;

public sealed class GetChronicDiseasesQueryHandler(IChronicDiseaseRepository diseases)
    : IRequestHandler<GetChronicDiseasesQuery, IReadOnlyList<ChronicDiseaseDto>>
{
    public async Task<IReadOnlyList<ChronicDiseaseDto>> Handle(GetChronicDiseasesQuery q,
        CancellationToken ct) => (await diseases.GetAllAsync(ct)).Select(d => d.ToDto()).ToList();
}
