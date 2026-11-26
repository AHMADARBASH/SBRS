using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Doctors.Commands;

public sealed record ActivateDoctorCommand(Guid Id) : IRequest;

public sealed class ActivateDoctorCommandValidator : AbstractValidator<ActivateDoctorCommand>
{
    public ActivateDoctorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class ActivateDoctorCommandHandler(IDoctorRepository doctors, IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateDoctorCommand>
{
    public async Task Handle(ActivateDoctorCommand c, CancellationToken ct)
    {
        var doctor = await doctors.GetByIdAsync(c.Id, ct)
                     ?? throw new NotFoundException(nameof(DoctorEntity), c.Id);

        doctor.Activate();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
