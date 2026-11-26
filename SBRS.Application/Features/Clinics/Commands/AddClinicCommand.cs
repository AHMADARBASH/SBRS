using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Clinics.DTOs;
using SBRS.Application.Features.Clinics.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Clinics.Commands;

public sealed record AddClinicCommand(Guid? FacilityId, Guid ClinicTypeId) : IRequest<ClinicDto>;

public sealed class AddClinicCommandValidator : AbstractValidator<AddClinicCommand>
{
    public AddClinicCommandValidator()
    {
        RuleFor(x => x.FacilityId).NotNull().WithMessage("Facility is required.")
            .NotEmpty().When(x => x.FacilityId.HasValue);
        RuleFor(x => x.ClinicTypeId).NotEmpty();
    }
}

public sealed class AddClinicCommandHandler(
    IFacilityRepository facilities,
    IClinicTypeRepository clinicTypes,
    IClinicRepository clinics,
    IUnitOfWork unitOfWork) : IRequestHandler<AddClinicCommand, ClinicDto>
{
    public async Task<ClinicDto> Handle(AddClinicCommand c, CancellationToken ct)
    {
        var facilityId = c.FacilityId!.Value;
        var facility = await facilities.GetByIdAsync(facilityId, ct)
                       ?? throw new NotFoundException(nameof(FacilityEntity), facilityId);
        if (!facility.IsActive)
            throw new DomainException("Cannot add a clinic to a deactivated facility.");

        var type = await clinicTypes.GetByIdAsync(c.ClinicTypeId, ct)
                   ?? throw new NotFoundException(nameof(ClinicTypeEntity), c.ClinicTypeId);

        var existing = await clinics.GetByFacilityAndTypeAsync(facilityId, type.Id, ct);
        if (existing is not null)
        {
            throw new ConflictException(existing.IsActive
                ? "This clinic type is already registered at the facility."
                : "This clinic type is already registered at the facility but inactive; activate the existing clinic instead.");
        }

        var clinic = new ClinicEntity(facility.Id, type.Id);
        await clinics.AddAsync(clinic, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return clinic.ToDto(type);
    }
}
