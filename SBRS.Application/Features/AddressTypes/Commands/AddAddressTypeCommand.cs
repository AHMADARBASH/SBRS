using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.AddressTypes.DTOs;
using SBRS.Application.Features.AddressTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.AddressTypes.Commands;

public sealed record AddAddressTypeCommand(string ArName, string EnName) : IRequest<AddressTypeDto>;

public sealed class AddAddressTypeCommandValidator : AbstractValidator<AddAddressTypeCommand>
{
    public AddAddressTypeCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
    }
}

public sealed class AddAddressTypeCommandHandler(
    IAddressTypeRepository types, IUnitOfWork unitOfWork) : IRequestHandler<AddAddressTypeCommand, AddressTypeDto>
{
    public async Task<AddressTypeDto> Handle(AddAddressTypeCommand c, CancellationToken ct)
    {
        var type = new AddressTypeEntity(c.ArName, c.EnName);
        await types.AddAsync(type, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return type.ToDto();
    }
}
