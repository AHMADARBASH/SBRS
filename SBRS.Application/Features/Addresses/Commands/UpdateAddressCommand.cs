using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Addresses.DTOs;
using SBRS.Application.Features.Addresses.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;
using System.Net;

namespace SBRS.Application.Features.Addresses.Commands;

public record UpdateAddressCommand(Guid Id, string ArName, string EnName) : IRequest<AddressDto>;

public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(200);
    }
}

public class UpdateAddressCommandHandler(IAddressRepository addresses, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAddressCommand, AddressDto>
{
    public async Task<AddressDto> Handle(UpdateAddressCommand request, CancellationToken ct)
    {
        var entity = await addresses.GetByIdAsync(request.Id, ct)
                     ?? throw new NotFoundException(nameof(AddressEntity), request.Id);

        var arName = request.ArName.Trim();
        var enName = request.EnName.Trim();

        if (await addresses.NameExistsAsync(entity.AreaId, arName, enName, entity.Id, ct))
            throw new ConflictException("An address with the same name already exists in this area.");

        entity.Update(arName, enName);
        await unitOfWork.SaveChangesAsync(ct);

        return entity.ToDto();
    }
}