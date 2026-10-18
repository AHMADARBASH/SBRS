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

public record UpdateFacilityCommand(Guid Id, Guid AddressId, string ArName, string EnName, string Code)
    : IRequest<FacilityDto>;

public class UpdateFacilityCommandValidator : AbstractValidator<UpdateFacilityCommand>
{
    public UpdateFacilityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}

public class UpdateFacilityCommandHandler(
    IFacilityRepository facilities,
    IAddressRepository addresses,
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

        var code = request.Code.Trim();

        if (await facilities.CodeExistsAsync(code, request.Id, ct))
            throw new ConflictException($"A facility with code '{code}' already exists.");

        entity.Update(request.ArName, request.EnName, code, address.Id);
        await unitOfWork.SaveChangesAsync(ct);

        return entity.ToDto();
    }
}