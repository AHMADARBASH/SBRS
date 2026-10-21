using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.DisabilityTypes.DTOs;
using SBRS.Application.Features.DisabilityTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.DisabilityTypes.Commands;

public sealed record AddDisabilityTypeCommand(string ArName, string EnName) : IRequest<DisabilityTypeDto>;

public sealed class AddDisabilityTypeCommandValidator : AbstractValidator<AddDisabilityTypeCommand>
{
    public AddDisabilityTypeCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
    }
}

public sealed class AddDisabilityTypeCommandHandler(IDisabilityTypeRepository disabilityTypes, IUnitOfWork uow)
    : IRequestHandler<AddDisabilityTypeCommand, DisabilityTypeDto>
{
    public async Task<DisabilityTypeDto> Handle(AddDisabilityTypeCommand c, CancellationToken ct)
    {
        var enName = c.EnName.Trim();
        if (await disabilityTypes.EnNameExistsAsync(enName, null, ct))
            throw new ConflictException($"Disability type '{enName}' already exists.");

        var disabilityType = new DisabilityTypeEntity(c.ArName, enName);
        await disabilityTypes.AddAsync(disabilityType, ct);
        await uow.SaveChangesAsync(ct);

        return disabilityType.ToDto();
    }
}
