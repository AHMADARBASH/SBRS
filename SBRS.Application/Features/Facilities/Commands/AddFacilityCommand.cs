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

public record AddFacilityCommand(Guid BranchId, Guid AddressId, string ArName, string EnName, string Code)
    : IRequest<FacilityDto>;

public class AddFacilityCommandValidator : AbstractValidator<AddFacilityCommand>
{
    public AddFacilityCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
    }
}

public class AddFacilityCommandHandler(
    IFacilityRepository facilities,
    IBranchRepository branches,
    IAddressRepository addresses,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddFacilityCommand, FacilityDto>
{
    public async Task<FacilityDto> Handle(AddFacilityCommand request, CancellationToken ct)
    {
        // scoped lookup: an OrganizationAdmin cannot add a facility to another organization's branch
        var branch = await branches.GetByIdAsync(request.BranchId, ct)
                     ?? throw new NotFoundException(nameof(BranchEntity), request.BranchId);

        if (!branch.IsActive)
            throw new DomainException("Cannot add a facility to a deactivated branch.");

        var address = await addresses.GetByIdAsync(request.AddressId, ct)
                      ?? throw new NotFoundException(nameof(AddressEntity), request.AddressId);

        if (!address.IsActive)
            throw new DomainException("Cannot use a deactivated address.");

        var code = request.Code.Trim();

        if (await facilities.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"A facility with code '{code}' already exists.");

        var entity = new FacilityEntity(branch.Id, address.Id, request.ArName, request.EnName, code);

        await facilities.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return entity.ToDto();
    }
}