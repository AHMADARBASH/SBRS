using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Addresses.DTOs;
using SBRS.Application.Features.Addresses.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;
using System.Net;

namespace SBRS.Application.Features.Addresses.Queries;

public record GetAddressByIdQuery(Guid Id) : IRequest<AddressDto>;

public class GetAddressByIdQueryHandler(IAddressRepository addresses)
    : IRequestHandler<GetAddressByIdQuery, AddressDto>
{
    public async Task<AddressDto> Handle(GetAddressByIdQuery request, CancellationToken ct)
    {
        var entity = await addresses.GetByIdAsync(request.Id, ct)
                     ?? throw new NotFoundException(nameof(AddressEntity), request.Id);

        return entity.ToDto();
    }
}