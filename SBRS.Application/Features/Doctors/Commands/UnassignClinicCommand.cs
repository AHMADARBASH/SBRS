using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Doctors.Commands;

public sealed record UnassignClinicCommand(Guid DoctorId, Guid ClinicId) : IRequest;

public sealed class UnassignClinicCommandValidator : AbstractValidator<UnassignClinicCommand>
{
    public UnassignClinicCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.ClinicId).NotEmpty();
    }
}

public sealed class UnassignClinicCommandHandler(
    IDoctorRepository doctors,
    IClinicRepository clinics,
    IClinicDoctorRepository links,
    IUnitOfWork unitOfWork) : IRequestHandler<UnassignClinicCommand>
{
    public async Task Handle(UnassignClinicCommand c, CancellationToken ct)
    {
        var doctor = await doctors.GetByIdAsync(c.DoctorId, ct)
                     ?? throw new NotFoundException(nameof(DoctorEntity), c.DoctorId);
        var clinic = await clinics.GetByIdAsync(c.ClinicId, ct)
                     ?? throw new NotFoundException(nameof(ClinicEntity), c.ClinicId);

        var link = await links.GetAsync(clinic.Id, doctor.Id, ct);
        if (link is null || !link.IsActive)
            throw new NotFoundException(nameof(ClinicDoctorEntity), $"{c.ClinicId}/{c.DoctorId}");

        link.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
