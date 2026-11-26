using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.AddressTypes.DTOs;
using SBRS.Application.Features.AddressTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.AddressTypes.Commands;

public sealed record UpdateAddressTypeCommand(Guid Id, string ArName, string EnName)
    : IRequest<AddressTypeDto>;

public sealed class UpdateAddressTypeCommandValidator : AbstractValidator<UpdateAddressTypeCommand>
{
    public UpdateAddressTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateAddressTypeCommandHandler(
    IAddressTypeRepository types, IUnitOfWork unitOfWork) : IRequestHandler<UpdateAddressTypeCommand, AddressTypeDto>
{
    public async Task<AddressTypeDto> Handle(UpdateAddressTypeCommand c, CancellationToken ct)
    {
        var type = await types.GetByIdAsync(c.Id, ct)
                   ?? throw new NotFoundException(nameof(AddressTypeEntity), c.Id);

        type.Update(c.ArName, c.EnName);
        await unitOfWork.SaveChangesAsync(ct);

        return type.ToDto();
    }
}
