using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Diseases.DTOs;
using SBRS.Application.Features.Diseases.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Diseases.Commands;

public sealed record UpdateDiseaseCommand(
    Guid Id, string ArName, string EnName, string Code, Guid DiseaseCategoryId) : IRequest<DiseaseDto>;

public sealed class UpdateDiseaseCommandValidator : AbstractValidator<UpdateDiseaseCommand>
{
    public UpdateDiseaseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DiseaseCategoryId).NotEmpty();
    }
}

public sealed class UpdateDiseaseCommandHandler(
    IDiseaseRepository diseases, IDiseaseCategoryRepository categories, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateDiseaseCommand, DiseaseDto>
{
    public async Task<DiseaseDto> Handle(UpdateDiseaseCommand c, CancellationToken ct)
    {
        var disease = await diseases.GetByIdAsync(c.Id, ct)
                      ?? throw new NotFoundException(nameof(DiseaseEntity), c.Id);

        var category = await categories.GetByIdAsync(c.DiseaseCategoryId, ct)
                       ?? throw new NotFoundException(nameof(DiseaseCategoryEntity), c.DiseaseCategoryId);

        var code = c.Code.Trim();
        if (await diseases.CodeExistsAsync(code, c.Id, ct))
            throw new ConflictException($"Disease code '{code}' already exists.");

        disease.Update(c.ArName, c.EnName, code, category.Id);
        await unitOfWork.SaveChangesAsync(ct);

        return disease.ToDto(category);
    }
}
