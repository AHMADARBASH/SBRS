using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.FacilityTypes.DTOs;
using SBRS.Application.Features.FacilityTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.FacilityTypes.Commands;

public sealed record AddFacilityTypeCommand(string ArName, string EnName, string Code) : IRequest<FacilityTypeDto>;

public sealed class AddFacilityTypeCommandValidator : AbstractValidator<AddFacilityTypeCommand>
{
    public AddFacilityTypeCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}

public sealed class AddFacilityTypeCommandHandler(IFacilityTypeRepository types, IUnitOfWork uow)
    : IRequestHandler<AddFacilityTypeCommand, FacilityTypeDto>
{
    public async Task<FacilityTypeDto> Handle(AddFacilityTypeCommand c, CancellationToken ct)
    {
        var code = c.Code.Trim();
        if (await types.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"Facility type code '{code}' already exists.");

        var type = new FacilityTypeEntity(c.ArName, c.EnName, code);
        await types.AddAsync(type, ct);
        await uow.SaveChangesAsync(ct);

        return type.ToDto();
    }
}