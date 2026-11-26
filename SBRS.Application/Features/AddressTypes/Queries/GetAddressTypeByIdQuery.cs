using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.AddressTypes.DTOs;
using SBRS.Application.Features.AddressTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.AddressTypes.Queries;

public sealed record GetAddressTypeByIdQuery(Guid Id) : IRequest<AddressTypeDto>;

public sealed class GetAddressTypeByIdQueryHandler(IAddressTypeRepository types)
    : IRequestHandler<GetAddressTypeByIdQuery, AddressTypeDto>
{
    public async Task<AddressTypeDto> Handle(GetAddressTypeByIdQuery q, CancellationToken ct)
    {
        var type = await types.GetByIdAsync(q.Id, ct)
                   ?? throw new NotFoundException(nameof(AddressTypeEntity), q.Id);

        return type.ToDto();
    }
}
