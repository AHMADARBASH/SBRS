using MediatR;
using SBRS.Application.Features.AddressTypes.DTOs;
using SBRS.Application.Features.AddressTypes.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.AddressTypes.Queries;

public sealed record GetAddressTypesQuery : IRequest<IReadOnlyList<AddressTypeDto>>;

public sealed class GetAddressTypesQueryHandler(IAddressTypeRepository types)
    : IRequestHandler<GetAddressTypesQuery, IReadOnlyList<AddressTypeDto>>
{
    public async Task<IReadOnlyList<AddressTypeDto>> Handle(GetAddressTypesQuery q, CancellationToken ct) =>
        (await types.GetAllAsync(ct)).Select(t => t.ToDto()).ToList();
}
