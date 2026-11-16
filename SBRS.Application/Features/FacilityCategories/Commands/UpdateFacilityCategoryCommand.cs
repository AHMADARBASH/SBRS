using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.FacilityCategories.DTOs;
using SBRS.Application.Features.FacilityCategories.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.FacilityCategories.Commands;

public sealed record UpdateFacilityCategoryCommand(Guid Id, string ArName, string EnName, string Code)
    : IRequest<FacilityCategoryDto>;

public sealed class UpdateFacilityCategoryCommandValidator : AbstractValidator<UpdateFacilityCategoryCommand>
{
    public UpdateFacilityCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}

public sealed class UpdateFacilityCategoryCommandHandler(
    IFacilityCategoryRepository categories, IUnitOfWork uow)
    : IRequestHandler<UpdateFacilityCategoryCommand, FacilityCategoryDto>
{
    public async Task<FacilityCategoryDto> Handle(UpdateFacilityCategoryCommand c, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(c.Id, ct)
                       ?? throw new NotFoundException(nameof(FacilityCategoryEntity), c.Id);

        var enName = c.EnName.Trim();
        if (await categories.EnNameExistsAsync(enName, c.Id, ct))
            throw new ConflictException($"Facility category '{enName}' already exists.");

        var code = c.Code.Trim();
        if (await categories.CodeExistsAsync(code, c.Id, ct))
            throw new ConflictException($"Facility category code '{code}' already exists.");

        category.Update(c.ArName, enName, code);
        await uow.SaveChangesAsync(ct);

        return category.ToDto();
    }
}
