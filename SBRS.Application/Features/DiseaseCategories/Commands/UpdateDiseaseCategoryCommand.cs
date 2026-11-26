using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.DiseaseCategories.DTOs;
using SBRS.Application.Features.DiseaseCategories.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.DiseaseCategories.Commands;

public sealed record UpdateDiseaseCategoryCommand(Guid Id, string ArName, string EnName, string Code)
    : IRequest<DiseaseCategoryDto>;

public sealed class UpdateDiseaseCategoryCommandValidator : AbstractValidator<UpdateDiseaseCategoryCommand>
{
    public UpdateDiseaseCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}

public sealed class UpdateDiseaseCategoryCommandHandler(
    IDiseaseCategoryRepository categories, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateDiseaseCategoryCommand, DiseaseCategoryDto>
{
    public async Task<DiseaseCategoryDto> Handle(UpdateDiseaseCategoryCommand c, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(c.Id, ct)
                       ?? throw new NotFoundException(nameof(DiseaseCategoryEntity), c.Id);

        var code = c.Code.Trim();
        if (await categories.CodeExistsAsync(code, c.Id, ct))
            throw new ConflictException($"Disease category code '{code}' already exists.");

        category.Update(c.ArName, c.EnName, code);
        await unitOfWork.SaveChangesAsync(ct);

        return category.ToDto();
    }
}
