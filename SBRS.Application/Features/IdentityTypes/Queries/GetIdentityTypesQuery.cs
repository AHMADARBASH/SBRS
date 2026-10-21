using MediatR;
using SBRS.Application.Features.IdentityTypes.DTOs;
using SBRS.Application.Features.IdentityTypes.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.IdentityTypes.Queries;

public sealed record GetIdentityTypesQuery : IRequest<IReadOnlyList<IdentityTypeDto>>;

public sealed class GetIdentityTypesQueryHandler(IIdentityTypeRepository identityTypes)
    : IRequestHandler<GetIdentityTypesQuery, IReadOnlyList<IdentityTypeDto>>
{
    public async Task<IReadOnlyList<IdentityTypeDto>> Handle(GetIdentityTypesQuery q, CancellationToken ct) =>
        (await identityTypes.GetAllAsync(ct)).Select(i => i.ToDto()).ToList();
}
