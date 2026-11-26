using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.ClinicTypes.DTOs;
using SBRS.Application.Features.ClinicTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.ClinicTypes.Commands;

public sealed record AddClinicTypeCommand(string ArName, string EnName, string Code)
    : IRequest<ClinicTypeDto>;

public sealed class AddClinicTypeCommandValidator : AbstractValidator<AddClinicTypeCommand>
{
    public AddClinicTypeCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}

public sealed class AddClinicTypeCommandHandler(IClinicTypeRepository types, IUnitOfWork unitOfWork)
    : IRequestHandler<AddClinicTypeCommand, ClinicTypeDto>
{
    public async Task<ClinicTypeDto> Handle(AddClinicTypeCommand c, CancellationToken ct)
    {
        var code = c.Code.Trim();
        if (await types.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"Clinic type code '{code}' already exists.");

        var type = new ClinicTypeEntity(c.ArName, c.EnName, code);
        await types.AddAsync(type, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return type.ToDto();
    }
}
