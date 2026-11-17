using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Facilities.DTOs;
using SBRS.Application.Features.Facilities.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;
using System.Net;

namespace SBRS.Application.Features.Facilities.Commands;

// No Code here: the generated code is immutable, it is never recomputed on edit.
public record UpdateFacilityCommand(Guid Id, Guid AddressId, Guid typeId, string ArName, string EnName, Guid? CategoryId = null)
    : IRequest<FacilityDto>;

public class UpdateFacilityCommandValidator : AbstractValidator<UpdateFacilityCommand>
{
    public UpdateFacilityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.typeId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();   // optional; rejects Guid.Empty when supplied
    }
}

public class UpdateFacilityCommandHandler(
    IFacilityRepository facilities,
    IAddressRepository addresses,
    IFacilityCategoryRepository facilityCategories,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateFacilityCommand, FacilityDto>
{
    public async Task<FacilityDto> Handle(UpdateFacilityCommand request, CancellationToken ct)
    {
        var entity = await facilities.GetByIdAsync(request.Id, ct)
                     ?? throw new NotFoundException(nameof(FacilityEntity), request.Id);

        var address = await addresses.GetByIdAsync(request.AddressId, ct)
                      ?? throw new NotFoundException(nameof(AddressEntity), request.AddressId);

        // only reject an inactive address when the facility is switching to it
        if (!address.IsActive && address.Id != entity.AddressId)
            throw new DomainException("Cannot use a deactivated address.");

        if (request.CategoryId is { } categoryId &&
            await facilityCategories.GetByIdAsync(categoryId, ct) is null)
            throw new NotFoundException(nameof(FacilityCategoryEntity), categoryId);

        // the code is not touched: it is immutable because beneficiary FullCodes are built from it
        entity.Update(request.ArName, request.EnName, address.Id, request.typeId, request.CategoryId);
        await unitOfWork.SaveChangesAsync(ct);

        return entity.ToDto();
    }
}
