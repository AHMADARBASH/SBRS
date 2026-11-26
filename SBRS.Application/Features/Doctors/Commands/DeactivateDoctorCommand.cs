using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Doctors.Commands;

public sealed record DeactivateDoctorCommand(Guid Id) : IRequest;

public sealed class DeactivateDoctorCommandValidator : AbstractValidator<DeactivateDoctorCommand>
{
    public DeactivateDoctorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class DeactivateDoctorCommandHandler(
    IDoctorRepository doctors,
    IClinicDoctorRepository links,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivateDoctorCommand>
{
    public async Task Handle(DeactivateDoctorCommand c, CancellationToken ct)
    {
        var doctor = await doctors.GetByIdAsync(c.Id, ct)
                     ?? throw new NotFoundException(nameof(DoctorEntity), c.Id);

        doctor.Deactivate();
        foreach (var link in await links.GetActiveByDoctorAsync(c.Id, ct))
            link.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);
    }
}
