using MediatR;
using SBRS.Application.Features.ClinicTypes.DTOs;
using SBRS.Application.Features.ClinicTypes.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.ClinicTypes.Queries;

public sealed record GetClinicTypesQuery : IRequest<IReadOnlyList<ClinicTypeDto>>;

public sealed class GetClinicTypesQueryHandler(IClinicTypeRepository types)
    : IRequestHandler<GetClinicTypesQuery, IReadOnlyList<ClinicTypeDto>>
{
    public async Task<IReadOnlyList<ClinicTypeDto>> Handle(GetClinicTypesQuery q, CancellationToken ct) =>
        (await types.GetAllAsync(ct)).Select(t => t.ToDto()).ToList();
}
