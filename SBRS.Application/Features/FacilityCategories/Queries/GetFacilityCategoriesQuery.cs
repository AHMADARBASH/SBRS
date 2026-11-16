using MediatR;
using SBRS.Application.Features.FacilityCategories.DTOs;
using SBRS.Application.Features.FacilityCategories.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.FacilityCategories.Queries;

public sealed record GetFacilityCategoriesQuery : IRequest<IReadOnlyList<FacilityCategoryDto>>;

public sealed class GetFacilityCategoriesQueryHandler(IFacilityCategoryRepository categories)
    : IRequestHandler<GetFacilityCategoriesQuery, IReadOnlyList<FacilityCategoryDto>>
{
    public async Task<IReadOnlyList<FacilityCategoryDto>> Handle(GetFacilityCategoriesQuery q, CancellationToken ct) =>
        (await categories.GetAllAsync(ct)).Select(c => c.ToDto()).ToList();
}
