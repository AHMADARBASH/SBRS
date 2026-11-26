using MediatR;
using SBRS.Application.Features.DiseaseCategories.DTOs;
using SBRS.Application.Features.DiseaseCategories.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.DiseaseCategories.Queries;

public sealed record GetDiseaseCategoriesQuery : IRequest<IReadOnlyList<DiseaseCategoryDto>>;

public sealed class GetDiseaseCategoriesQueryHandler(IDiseaseCategoryRepository categories)
    : IRequestHandler<GetDiseaseCategoriesQuery, IReadOnlyList<DiseaseCategoryDto>>
{
    public async Task<IReadOnlyList<DiseaseCategoryDto>> Handle(GetDiseaseCategoriesQuery q, CancellationToken ct) =>
        (await categories.GetAllAsync(ct)).Select(c => c.ToDto()).ToList();
}
