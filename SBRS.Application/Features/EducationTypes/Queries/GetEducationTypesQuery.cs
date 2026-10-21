using MediatR;
using SBRS.Application.Features.EducationTypes.DTOs;
using SBRS.Application.Features.EducationTypes.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.EducationTypes.Queries;

public sealed record GetEducationTypesQuery : IRequest<IReadOnlyList<EducationTypeDto>>;

public sealed class GetEducationTypesQueryHandler(IEducationTypeRepository educationTypes)
    : IRequestHandler<GetEducationTypesQuery, IReadOnlyList<EducationTypeDto>>
{
    public async Task<IReadOnlyList<EducationTypeDto>> Handle(GetEducationTypesQuery q, CancellationToken ct) =>
        (await educationTypes.GetAllAsync(ct)).Select(e => e.ToDto()).ToList();
}
