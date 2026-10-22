using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.IdentityTypes.DTOs;
using SBRS.Application.Features.IdentityTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.IdentityTypes.Commands;

public sealed record UpdateIdentityTypeCommand(Guid Id, string ArName, string EnName, string Code)
    : IRequest<IdentityTypeDto>;

public sealed class UpdateIdentityTypeCommandValidator : AbstractValidator<UpdateIdentityTypeCommand>
{
    public UpdateIdentityTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
    }
}

public sealed class UpdateIdentityTypeCommandHandler(IIdentityTypeRepository identityTypes, IUnitOfWork uow)
    : IRequestHandler<UpdateIdentityTypeCommand, IdentityTypeDto>
{
    public async Task<IdentityTypeDto> Handle(UpdateIdentityTypeCommand c, CancellationToken ct)
    {
        var identityType = await identityTypes.GetByIdAsync(c.Id, ct)
                           ?? throw new NotFoundException(nameof(IdentityTypeEntity), c.Id);

        var enName = c.EnName.Trim();
        var code = c.Code.Trim();
        if (await identityTypes.EnNameExistsAsync(enName, c.Id, ct))
            throw new ConflictException($"Identity type '{enName}' already exists.");
        if (await identityTypes.CodeExistsAsync(code, c.Id, ct))
            throw new ConflictException($"Identity type code '{code}' already exists.");

        identityType.Update(c.ArName, enName, code);
        await uow.SaveChangesAsync(ct);

        return identityType.ToDto();
    }
}
