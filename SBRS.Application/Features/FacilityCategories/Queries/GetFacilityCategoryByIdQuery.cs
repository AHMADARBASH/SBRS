using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.FacilityCategories.DTOs;
using SBRS.Application.Features.FacilityCategories.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.FacilityCategories.Queries;

public sealed record GetFacilityCategoryByIdQuery(Guid Id) : IRequest<FacilityCategoryDto>;

public sealed class GetFacilityCategoryByIdQueryHandler(IFacilityCategoryRepository categories)
    : IRequestHandler<GetFacilityCategoryByIdQuery, FacilityCategoryDto>
{
    public async Task<FacilityCategoryDto> Handle(GetFacilityCategoryByIdQuery q, CancellationToken ct)
    {
        var entity = await categories.GetByIdAsync(q.Id, ct)
                     ?? throw new NotFoundException(nameof(FacilityCategoryEntity), q.Id);

        return entity.ToDto();
    }
}
