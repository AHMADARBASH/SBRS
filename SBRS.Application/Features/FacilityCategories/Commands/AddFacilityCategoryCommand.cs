using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.FacilityCategories.DTOs;
using SBRS.Application.Features.FacilityCategories.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.FacilityCategories.Commands;

public sealed record AddFacilityCategoryCommand(string ArName, string EnName, string Code)
    : IRequest<FacilityCategoryDto>;

public sealed class AddFacilityCategoryCommandValidator : AbstractValidator<AddFacilityCategoryCommand>
{
    public AddFacilityCategoryCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}

public sealed class AddFacilityCategoryCommandHandler(
    IFacilityCategoryRepository categories, IUnitOfWork uow)
    : IRequestHandler<AddFacilityCategoryCommand, FacilityCategoryDto>
{
    public async Task<FacilityCategoryDto> Handle(AddFacilityCategoryCommand c, CancellationToken ct)
    {
        var enName = c.EnName.Trim();
        if (await categories.EnNameExistsAsync(enName, null, ct))
            throw new ConflictException($"Facility category '{enName}' already exists.");

        var code = c.Code.Trim();
        if (await categories.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"Facility category code '{code}' already exists.");

        var category = new FacilityCategoryEntity(c.ArName, enName, code);
        await categories.AddAsync(category, ct);
        await uow.SaveChangesAsync(ct);

        return category.ToDto();
    }
}
