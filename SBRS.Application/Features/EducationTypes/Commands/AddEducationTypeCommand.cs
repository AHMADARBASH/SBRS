using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.EducationTypes.DTOs;
using SBRS.Application.Features.EducationTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.EducationTypes.Commands;

public sealed record AddEducationTypeCommand(string ArName, string EnName) : IRequest<EducationTypeDto>;

public sealed class AddEducationTypeCommandValidator : AbstractValidator<AddEducationTypeCommand>
{
    public AddEducationTypeCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
    }
}

public sealed class AddEducationTypeCommandHandler(IEducationTypeRepository educationTypes, IUnitOfWork uow)
    : IRequestHandler<AddEducationTypeCommand, EducationTypeDto>
{
    public async Task<EducationTypeDto> Handle(AddEducationTypeCommand c, CancellationToken ct)
    {
        var enName = c.EnName.Trim();
        if (await educationTypes.EnNameExistsAsync(enName, null, ct))
            throw new ConflictException($"Education type '{enName}' already exists.");

        var educationType = new EducationTypeEntity(c.ArName, enName);
        await educationTypes.AddAsync(educationType, ct);
        await uow.SaveChangesAsync(ct);

        return educationType.ToDto();
    }
}
