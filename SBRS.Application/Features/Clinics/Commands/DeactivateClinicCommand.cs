using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Clinics.Commands;

public sealed record DeactivateClinicCommand(Guid Id) : IRequest;

public sealed class DeactivateClinicCommandValidator : AbstractValidator<DeactivateClinicCommand>
{
    public DeactivateClinicCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class DeactivateClinicCommandHandler(IClinicRepository clinics, IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateClinicCommand>
{
    public async Task Handle(DeactivateClinicCommand c, CancellationToken ct)
    {
        var clinic = await clinics.GetByIdAsync(c.Id, ct)
                     ?? throw new NotFoundException(nameof(ClinicEntity), c.Id);

        if (await clinics.HasActiveDoctorLinksAsync(c.Id, ct))
            throw new DomainException(
                "The clinic cannot be deactivated while it has active doctor assignments.");

        clinic.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
