using MediatR;
using SBRS.Application.Features.Diseases.DTOs;
using SBRS.Application.Features.Diseases.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Diseases.Queries;

public sealed record GetDiseasesQuery(Guid? DiseaseCategoryId) : IRequest<IReadOnlyList<DiseaseDto>>;

public sealed class GetDiseasesQueryHandler(IDiseaseRepository diseases)
    : IRequestHandler<GetDiseasesQuery, IReadOnlyList<DiseaseDto>>
{
    public async Task<IReadOnlyList<DiseaseDto>> Handle(GetDiseasesQuery q, CancellationToken ct) =>
        (await diseases.GetAllAsync(q.DiseaseCategoryId, ct)).Select(d => d.ToDto()).ToList();
}
