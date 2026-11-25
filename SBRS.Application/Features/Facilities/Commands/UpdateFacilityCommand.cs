using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Facilities.DTOs;
using SBRS.Application.Features.Facilities.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Facilities.Commands;

// No Code here: the generated code is immutable, it is never recomputed on edit.
public record UpdateFacilityCommand(Guid Id, Guid AreaId, Guid typeId, string ArName, string EnName, Guid? CategoryId = null)
    : IRequest<FacilityDto>;

public class UpdateFacilityCommandValidator : AbstractValidator<UpdateFacilityCommand>
{
    public UpdateFacilityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AreaId).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.typeId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();   // optional; rejects Guid.Empty when supplied
    }
}

public class UpdateFacilityCommandHandler(
    IFacilityRepository facilities,
    IGeographyRepository geography,
    IFacilityCategoryRepository facilityCategories,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateFacilityCommand, FacilityDto>
{
    public async Task<FacilityDto> Handle(UpdateFacilityCommand request, CancellationToken ct)
    {
        var entity = await facilities.GetByIdAsync(request.Id, ct)
                     ?? throw new NotFoundException(nameof(FacilityEntity), request.Id);

        // Areas are read-only OCHA geography and carry no active flag, so existence is the only check.
        var area = await geography.GetAreaByIdAsync(request.AreaId, ct)
                   ?? throw new NotFoundException(nameof(AreaEntity), request.AreaId);

        if (request.CategoryId is { } categoryId &&
            await facilityCategories.GetByIdAsync(categoryId, ct) is null)
            throw new NotFoundException(nameof(FacilityCategoryEntity), categoryId);

        // the code is not touched: it is immutable because beneficiary FullCodes are built from it
        entity.Update(request.ArName, request.EnName, area.Id, request.typeId, request.CategoryId);
        await unitOfWork.SaveChangesAsync(ct);

        // The update changed the area/type/category ids, so reload with the related objects
        // instead of using the stale navigations of the tracked entity.
        return (await facilities.GetByIdWithDetailsAsync(entity.Id, ct)
                ?? throw new NotFoundException(nameof(FacilityEntity), entity.Id))
               .ToDto();
    }
}
