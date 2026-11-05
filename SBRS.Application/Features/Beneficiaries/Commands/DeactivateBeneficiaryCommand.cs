using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Commands;

/// <summary>Soft deletes the beneficiary: IsActive = false. Never a hard delete — codes are never reused.</summary>
public sealed record DeactivateBeneficiaryCommand(Guid Id) : IRequest;

public sealed class DeactivateBeneficiaryCommandHandler(
    IBeneficiaryRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivateBeneficiaryCommand>
{
    public async Task Handle(DeactivateBeneficiaryCommand request, CancellationToken cancellationToken)
    {
        var beneficiary = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (beneficiary is null ||
            (!currentUser.IsSystemAdmin && beneficiary.RegistrationFacilityId != currentUser.FacilityId))
        {
            // Out-of-scope records look like missing ones (same rule as the other features).
            throw new NotFoundException(nameof(BeneficiaryEntity), request.Id);
        }

        // Deactivate instead of deleting: printed codes, match history and the sequence stay valid.
        beneficiary.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
