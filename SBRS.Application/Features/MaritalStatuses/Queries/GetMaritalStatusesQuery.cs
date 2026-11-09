using MediatR;
using SBRS.Application.Features.MaritalStatuses.DTOs;
using SBRS.Application.Features.MaritalStatuses.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.MaritalStatuses.Queries;

public sealed record GetMaritalStatusesQuery : IRequest<IReadOnlyList<MaritalStatusDto>>;

public sealed class GetMaritalStatusesQueryHandler(IMaritalStatusRepository statuses)
    : IRequestHandler<GetMaritalStatusesQuery, IReadOnlyList<MaritalStatusDto>>
{
    public async Task<IReadOnlyList<MaritalStatusDto>> Handle(GetMaritalStatusesQuery q,
        CancellationToken ct) => (await statuses.GetAllAsync(ct)).Select(s => s.ToDto()).ToList();
}
