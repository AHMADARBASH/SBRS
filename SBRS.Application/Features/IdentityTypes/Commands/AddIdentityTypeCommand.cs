using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.IdentityTypes.DTOs;
using SBRS.Application.Features.IdentityTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.IdentityTypes.Commands;

public sealed record AddIdentityTypeCommand(string ArName, string EnName, string Code) : IRequest<IdentityTypeDto>;

public sealed class AddIdentityTypeCommandValidator : AbstractValidator<AddIdentityTypeCommand>
{
    public AddIdentityTypeCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
    }
}

public sealed class AddIdentityTypeCommandHandler(IIdentityTypeRepository identityTypes, IUnitOfWork uow)
    : IRequestHandler<AddIdentityTypeCommand, IdentityTypeDto>
{
    public async Task<IdentityTypeDto> Handle(AddIdentityTypeCommand c, CancellationToken ct)
    {
        var enName = c.EnName.Trim();
        var code = c.Code.Trim();
        if (await identityTypes.EnNameExistsAsync(enName, null, ct))
            throw new ConflictException($"Identity type '{enName}' already exists.");
        if (await identityTypes.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"Identity type code '{code}' already exists.");

        var identityType = new IdentityTypeEntity(c.ArName, enName, code);
        await identityTypes.AddAsync(identityType, ct);
        await uow.SaveChangesAsync(ct);

        return identityType.ToDto();
    }
}
