using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.WorkingAddresses.DTOs;
using SBRS.Application.Features.WorkingAddresses.Mappings;
using SBRS.Application.Features.Visits;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.WorkingAddresses.Commands;

public sealed record NewWorkingAddress(Guid AreaId, string ArName, string EnName);

public sealed record AddWorkingAddressCommand(
    Guid FacilityId,
    DateOnly? WorkingDate,
    Guid TypeId,
    Guid? AddressId,
    NewWorkingAddress? NewAddress) : IRequest<WorkingAddressDto>;

public sealed class NewWorkingAddressValidator : AbstractValidator<NewWorkingAddress>
{
    public NewWorkingAddressValidator()
    {
        RuleFor(x => x.AreaId).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(200);
    }
}

public sealed class AddWorkingAddressCommandValidator : AbstractValidator<AddWorkingAddressCommand>
{
    public AddWorkingAddressCommandValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.TypeId).NotEmpty();
        RuleFor(x => x)
            .Must(x => x.AddressId.HasValue != (x.NewAddress is not null))
            .WithMessage("Send either AddressId or NewAddress, not both.");
        RuleFor(x => x.AddressId).NotEmpty().When(x => x.AddressId.HasValue);
        RuleFor(x => x.NewAddress).SetValidator(new NewWorkingAddressValidator())
            .When(x => x.NewAddress is not null);
    }
}

public sealed class AddWorkingAddressCommandHandler(
    IWorkingAddressRepository workingAddresses,
    IAddressTypeRepository types,
    IAddressRepository addresses,
    IGeographyRepository geography,
    IUnitOfWork unitOfWork) : IRequestHandler<AddWorkingAddressCommand, WorkingAddressDto>
{
    public async Task<WorkingAddressDto> Handle(AddWorkingAddressCommand c, CancellationToken ct)
    {
        _ = await types.GetByIdAsync(c.TypeId, ct)
            ?? throw new NotFoundException(nameof(AddressTypeEntity), c.TypeId);

        var workingDate = c.WorkingDate ?? VisitsDate.Today;
        Guid addressId;

        if (c.AddressId is { } existingId)
        {
            var address = await addresses.GetByIdAsync(existingId, ct)
                          ?? throw new NotFoundException(nameof(AddressEntity), existingId);
            if (!address.IsActive)
                throw new DomainException("The selected address is deactivated.");

            addressId = address.Id;
        }
        else
        {
            var newAddress = c.NewAddress!;
            var area = await geography.GetAreaByIdAsync(newAddress.AreaId, ct)
                       ?? throw new NotFoundException(nameof(AreaEntity), newAddress.AreaId);

            var arName = newAddress.ArName.Trim();
            var enName = newAddress.EnName.Trim();
            var inArea = await addresses.GetByAreaAsync(area.Id, includeInactive: true, ct);

            var exact = inArea.FirstOrDefault(a =>
                a.IsActive && a.ArName == arName && a.EnName == enName);
            if (exact is not null)
            {
                addressId = exact.Id;
            }
            else if (inArea.Any(a => a.ArName == arName || a.EnName == enName))
            {
                throw new ConflictException(
                    "An address with the same Arabic or English name already exists in this area.");
            }
            else
            {
                var address = new AddressEntity(area.Id, arName, enName);
                await addresses.AddAsync(address, ct);
                addressId = address.Id;
            }
        }

        if (await workingAddresses.DuplicateActiveAsync(c.FacilityId, addressId, workingDate, null, ct))
            throw new ConflictException(
                "An active working address already exists for this address and working date.");

        var entity = new WorkingAddressEntity(c.FacilityId, addressId, workingDate, c.TypeId);
        await workingAddresses.AddAsync(entity, ct);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (UniqueViolationException)
        {
            throw new ConflictException(
                "An address with the same Arabic or English name already exists in this area.");
        }

        return (await workingAddresses.GetByIdAsync(entity.Id, c.FacilityId, ct)
                ?? throw new NotFoundException(nameof(WorkingAddressEntity), entity.Id)).ToDto();
    }
}
