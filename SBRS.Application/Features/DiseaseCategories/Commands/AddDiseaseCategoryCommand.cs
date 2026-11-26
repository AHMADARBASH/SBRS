using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.DiseaseCategories.DTOs;
using SBRS.Application.Features.DiseaseCategories.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.DiseaseCategories.Commands;

public sealed record AddDiseaseCategoryCommand(string ArName, string EnName, string Code)
    : IRequest<DiseaseCategoryDto>;

public sealed class AddDiseaseCategoryCommandValidator : AbstractValidator<AddDiseaseCategoryCommand>
{
    public AddDiseaseCategoryCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}

public sealed class AddDiseaseCategoryCommandHandler(
    IDiseaseCategoryRepository categories, IUnitOfWork unitOfWork)
    : IRequestHandler<AddDiseaseCategoryCommand, DiseaseCategoryDto>
{
    public async Task<DiseaseCategoryDto> Handle(AddDiseaseCategoryCommand c, CancellationToken ct)
    {
        var code = c.Code.Trim();
        if (await categories.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"Disease category code '{code}' already exists.");

        var category = new DiseaseCategoryEntity(c.ArName, c.EnName, code);
        await categories.AddAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return category.ToDto();
    }
}
