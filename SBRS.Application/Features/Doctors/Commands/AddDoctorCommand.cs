using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Doctors.DTOs;
using SBRS.Application.Features.Doctors.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Doctors.Commands;

public sealed record AddDoctorCommand(Guid? FacilityId, string FirstName, string LastName)
    : IRequest<DoctorDto>;

public sealed class AddDoctorCommandValidator : AbstractValidator<AddDoctorCommand>
{
    public AddDoctorCommandValidator()
    {
        RuleFor(x => x.FacilityId).NotNull().WithMessage("Facility is required.")
            .NotEmpty().When(x => x.FacilityId.HasValue);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}

public sealed class AddDoctorCommandHandler(
    IFacilityRepository facilities,
    IDoctorRepository doctors,
    IUnitOfWork unitOfWork) : IRequestHandler<AddDoctorCommand, DoctorDto>
{
    public async Task<DoctorDto> Handle(AddDoctorCommand c, CancellationToken ct)
    {
        var facilityId = c.FacilityId!.Value;
        var facility = await facilities.GetByIdAsync(facilityId, ct)
                       ?? throw new NotFoundException(nameof(FacilityEntity), facilityId);
        if (!facility.IsActive)
            throw new DomainException("Cannot add a doctor to a deactivated facility.");

        var doctor = new DoctorEntity(facility.Id, c.FirstName, c.LastName);
        await doctors.AddAsync(doctor, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return doctor.ToDto();
    }
}
