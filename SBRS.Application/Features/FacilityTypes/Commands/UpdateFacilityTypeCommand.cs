using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.FacilityTypes.DTOs;
using SBRS.Application.Features.FacilityTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.FacilityTypes.Commands;

public sealed record UpdateFacilityTypeCommand(Guid Id, string ArName, string EnName, string Code)
    : IRequest<FacilityTypeDto>;

public sealed class UpdateFacilityTypeCommandValidator : AbstractValidator<UpdateFacilityTypeCommand>
{
    public UpdateFacilityTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
    }
}

public sealed class UpdateFacilityTypeCommandHandler(IFacilityTypeRepository types, IUnitOfWork uow)
    : IRequestHandler<UpdateFacilityTypeCommand, FacilityTypeDto>
{
    public async Task<FacilityTypeDto> Handle(UpdateFacilityTypeCommand c, CancellationToken ct)
    {
        var type = await types.GetByIdAsync(c.Id, ct)
                   ?? throw new NotFoundException(nameof(FacilityTypeEntity), c.Id);

        var code = c.Code.Trim();
        if (await types.CodeExistsAsync(code, c.Id, ct))
            throw new ConflictException($"Facility type code '{code}' already exists.");

        type.Update(c.ArName, c.EnName, code);
        await uow.SaveChangesAsync(ct);

        return type.ToDto();
    }
}