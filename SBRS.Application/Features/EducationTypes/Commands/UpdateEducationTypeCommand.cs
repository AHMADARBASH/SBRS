using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.EducationTypes.DTOs;
using SBRS.Application.Features.EducationTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.EducationTypes.Commands;

public sealed record UpdateEducationTypeCommand(Guid Id, string ArName, string EnName)
    : IRequest<EducationTypeDto>;

public sealed class UpdateEducationTypeCommandValidator : AbstractValidator<UpdateEducationTypeCommand>
{
    public UpdateEducationTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateEducationTypeCommandHandler(IEducationTypeRepository educationTypes, IUnitOfWork uow)
    : IRequestHandler<UpdateEducationTypeCommand, EducationTypeDto>
{
    public async Task<EducationTypeDto> Handle(UpdateEducationTypeCommand c, CancellationToken ct)
    {
        var educationType = await educationTypes.GetByIdAsync(c.Id, ct)
                            ?? throw new NotFoundException(nameof(EducationTypeEntity), c.Id);

        var enName = c.EnName.Trim();
        if (await educationTypes.EnNameExistsAsync(enName, c.Id, ct))
            throw new ConflictException($"Education type '{enName}' already exists.");

        educationType.Update(c.ArName, enName);
        await uow.SaveChangesAsync(ct);

        return educationType.ToDto();
    }
}
