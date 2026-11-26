using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.WorkingAddresses.DTOs;
using SBRS.Application.Features.WorkingAddresses.Mappings;
using SBRS.Application.Features.Visits;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.WorkingAddresses.Commands;

public sealed record UpdateWorkingAddressCommand(
    Guid Id, Guid FacilityId, DateOnly? WorkingDate, Guid TypeId) : IRequest<WorkingAddressDto>;

public sealed class UpdateWorkingAddressCommandValidator : AbstractValidator<UpdateWorkingAddressCommand>
{
    public UpdateWorkingAddressCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.TypeId).NotEmpty();
    }
}

public sealed class UpdateWorkingAddressCommandHandler(
    IWorkingAddressRepository workingAddresses,
    IAddressTypeRepository types,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateWorkingAddressCommand, WorkingAddressDto>
{
    public async Task<WorkingAddressDto> Handle(UpdateWorkingAddressCommand c, CancellationToken ct)
    {
        var entity = await workingAddresses.GetByIdTrackedAsync(c.Id, c.FacilityId, ct)
                     ?? throw new NotFoundException(nameof(WorkingAddressEntity), c.Id);

        _ = await types.GetByIdAsync(c.TypeId, ct)
            ?? throw new NotFoundException(nameof(AddressTypeEntity), c.TypeId);

        var workingDate = c.WorkingDate ?? VisitsDate.Today;
        if (await workingAddresses.DuplicateActiveAsync(
                c.FacilityId, entity.AddressId, workingDate, c.Id, ct))
            throw new ConflictException(
                "An active working address already exists for this address and working date.");

        entity.Update(workingDate, c.TypeId);
        await unitOfWork.SaveChangesAsync(ct);

        return (await workingAddresses.GetByIdAsync(c.Id, c.FacilityId, ct)
                ?? throw new NotFoundException(nameof(WorkingAddressEntity), c.Id)).ToDto();
    }
}
