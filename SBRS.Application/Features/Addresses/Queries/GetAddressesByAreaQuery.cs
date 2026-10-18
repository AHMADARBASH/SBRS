using MediatR;
using SBRS.Application.Features.Addresses.DTOs;
using SBRS.Application.Features.Addresses.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Addresses.Queries;

public record GetAddressesByAreaQuery(Guid AreaId, bool IncludeInactive = false)
    : IRequest<List<AddressDto>>;

public class GetAddressesByAreaQueryHandler(IAddressRepository addresses)
    : IRequestHandler<GetAddressesByAreaQuery, List<AddressDto>>
{
    public async Task<List<AddressDto>> Handle(GetAddressesByAreaQuery request, CancellationToken ct)
    {
        var items = await addresses.GetByAreaAsync(request.AreaId, request.IncludeInactive, ct);
        return items.Select(a => a.ToDto()).ToList();
    }
}