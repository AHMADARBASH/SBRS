using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.DisabilityTypes.DTOs;
using SBRS.Application.Features.DisabilityTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.DisabilityTypes.Commands;

public sealed record UpdateDisabilityTypeCommand(Guid Id, string ArName, string EnName)
    : IRequest<DisabilityTypeDto>;

public sealed class UpdateDisabilityTypeCommandValidator : AbstractValidator<UpdateDisabilityTypeCommand>
{
    public UpdateDisabilityTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateDisabilityTypeCommandHandler(IDisabilityTypeRepository disabilityTypes, IUnitOfWork uow)
    : IRequestHandler<UpdateDisabilityTypeCommand, DisabilityTypeDto>
{
    public async Task<DisabilityTypeDto> Handle(UpdateDisabilityTypeCommand c, CancellationToken ct)
    {
        var disabilityType = await disabilityTypes.GetByIdAsync(c.Id, ct)
                             ?? throw new NotFoundException(nameof(DisabilityTypeEntity), c.Id);

        var enName = c.EnName.Trim();
        if (await disabilityTypes.EnNameExistsAsync(enName, c.Id, ct))
            throw new ConflictException($"Disability type '{enName}' already exists.");

        disabilityType.Update(c.ArName, enName);
        await uow.SaveChangesAsync(ct);

        return disabilityType.ToDto();
    }
}
