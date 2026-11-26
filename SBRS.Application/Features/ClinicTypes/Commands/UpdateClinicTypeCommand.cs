using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.ClinicTypes.DTOs;
using SBRS.Application.Features.ClinicTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.ClinicTypes.Commands;

public sealed record UpdateClinicTypeCommand(Guid Id, string ArName, string EnName, string Code)
    : IRequest<ClinicTypeDto>;

public sealed class UpdateClinicTypeCommandValidator : AbstractValidator<UpdateClinicTypeCommand>
{
    public UpdateClinicTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}

public sealed class UpdateClinicTypeCommandHandler(IClinicTypeRepository types, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateClinicTypeCommand, ClinicTypeDto>
{
    public async Task<ClinicTypeDto> Handle(UpdateClinicTypeCommand c, CancellationToken ct)
    {
        var type = await types.GetByIdAsync(c.Id, ct)
                   ?? throw new NotFoundException(nameof(ClinicTypeEntity), c.Id);

        var code = c.Code.Trim();
        if (await types.CodeExistsAsync(code, c.Id, ct))
            throw new ConflictException($"Clinic type code '{code}' already exists.");

        type.Update(c.ArName, c.EnName, code);
        await unitOfWork.SaveChangesAsync(ct);

        return type.ToDto();
    }
}
