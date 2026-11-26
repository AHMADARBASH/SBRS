using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Doctors.Commands;

public sealed record AssignClinicCommand(Guid DoctorId, Guid ClinicId) : IRequest;

public sealed class AssignClinicCommandValidator : AbstractValidator<AssignClinicCommand>
{
    public AssignClinicCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.ClinicId).NotEmpty();
    }
}

public sealed class AssignClinicCommandHandler(
    IDoctorRepository doctors,
    IClinicRepository clinics,
    IClinicDoctorRepository links,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignClinicCommand>
{
    public async Task Handle(AssignClinicCommand c, CancellationToken ct)
    {
        var doctor = await doctors.GetByIdAsync(c.DoctorId, ct)
                     ?? throw new NotFoundException(nameof(DoctorEntity), c.DoctorId);
        var clinic = await clinics.GetByIdAsync(c.ClinicId, ct)
                     ?? throw new NotFoundException(nameof(ClinicEntity), c.ClinicId);

        if (!doctor.IsActive)
            throw new DomainException("A deactivated doctor cannot be assigned to a clinic.");
        if (!clinic.IsActive)
            throw new DomainException("A deactivated clinic cannot receive doctors.");
        if (doctor.FacilityId != clinic.FacilityId)
            throw new DomainException("The doctor and the clinic must belong to the same facility.");

        var existing = await links.GetAsync(c.ClinicId, c.DoctorId, ct);
        if (existing is null)
            await links.AddAsync(new ClinicDoctorEntity(clinic.Id, doctor.Id), ct);
        else if (existing.IsActive)
            throw new ConflictException("The doctor is already assigned to this clinic.");
        else
            existing.Activate();

        await unitOfWork.SaveChangesAsync(ct);
    }
}
