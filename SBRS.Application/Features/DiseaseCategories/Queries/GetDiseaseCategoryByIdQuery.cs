using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.DiseaseCategories.DTOs;
using SBRS.Application.Features.DiseaseCategories.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.DiseaseCategories.Queries;

public sealed record GetDiseaseCategoryByIdQuery(Guid Id) : IRequest<DiseaseCategoryDto>;

public sealed class GetDiseaseCategoryByIdQueryHandler(IDiseaseCategoryRepository categories)
    : IRequestHandler<GetDiseaseCategoryByIdQuery, DiseaseCategoryDto>
{
    public async Task<DiseaseCategoryDto> Handle(GetDiseaseCategoryByIdQuery q, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(q.Id, ct)
                       ?? throw new NotFoundException(nameof(DiseaseCategoryEntity), q.Id);

        return category.ToDto();
    }
}
