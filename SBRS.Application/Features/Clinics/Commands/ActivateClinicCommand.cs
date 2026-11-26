using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Clinics.Commands;

public sealed record ActivateClinicCommand(Guid Id) : IRequest;

public sealed class ActivateClinicCommandValidator : AbstractValidator<ActivateClinicCommand>
{
    public ActivateClinicCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class ActivateClinicCommandHandler(IClinicRepository clinics, IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateClinicCommand>
{
    public async Task Handle(ActivateClinicCommand c, CancellationToken ct)
    {
        var clinic = await clinics.GetByIdAsync(c.Id, ct)
                     ?? throw new NotFoundException(nameof(ClinicEntity), c.Id);

        clinic.Activate();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
