using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Diseases.DTOs;
using SBRS.Application.Features.Diseases.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Diseases.Commands;

public sealed record AddDiseaseCommand(
    string ArName, string EnName, string Code, Guid DiseaseCategoryId) : IRequest<DiseaseDto>;

public sealed class AddDiseaseCommandValidator : AbstractValidator<AddDiseaseCommand>
{
    public AddDiseaseCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DiseaseCategoryId).NotEmpty();
    }
}

public sealed class AddDiseaseCommandHandler(
    IDiseaseRepository diseases, IDiseaseCategoryRepository categories, IUnitOfWork unitOfWork)
    : IRequestHandler<AddDiseaseCommand, DiseaseDto>
{
    public async Task<DiseaseDto> Handle(AddDiseaseCommand c, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(c.DiseaseCategoryId, ct)
                       ?? throw new NotFoundException(nameof(DiseaseCategoryEntity), c.DiseaseCategoryId);

        var code = c.Code.Trim();
        if (await diseases.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"Disease code '{code}' already exists.");

        var disease = new DiseaseEntity(c.ArName, c.EnName, code, category.Id);
        await diseases.AddAsync(disease, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return disease.ToDto(category);
    }
}
