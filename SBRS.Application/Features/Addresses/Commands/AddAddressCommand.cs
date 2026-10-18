using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Addresses.DTOs;
using SBRS.Application.Features.Addresses.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;
using System.Net;

namespace SBRS.Application.Features.Addresses.Commands;

public record AddAddressCommand(Guid AreaId, string ArName, string EnName) : IRequest<AddressDto>;

public class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.AreaId).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(200);
    }
}

public class AddAddressCommandHandler(
    IAddressRepository addresses,
    IGeographyRepository geography,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddAddressCommand, AddressDto>
{
    public async Task<AddressDto> Handle(AddAddressCommand request, CancellationToken ct)
    {
        var area = await geography.GetAreaByIdAsync(request.AreaId, ct)
                   ?? throw new NotFoundException(nameof(AreaEntity), request.AreaId);


        var arName = request.ArName.Trim();
        var enName = request.EnName.Trim();

        if (await addresses.NameExistsAsync(area.Id, arName, enName, null, ct))
            throw new ConflictException("An address with the same name already exists in this area.");

        var entity = new AddressEntity(area.Id, arName, enName);

        await addresses.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return entity.ToDto();
    }
}