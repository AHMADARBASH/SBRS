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
    IFacilityBeneficiaryRepository enrollments,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivateBeneficiaryCommand>
{
    public async Task Handle(DeactivateBeneficiaryCommand request, CancellationToken cancellationToken)
    {
        var beneficiary = await repository.GetByIdAsync(request.Id, cancellationToken);

        // Deletion follows the roster: the person is visible to a facility through its enrollment,
        // not only through the facility where he was originally registered.
        var isInScope = currentUser.IsSystemAdmin ||
                        (currentUser.FacilityId is { } facilityId &&
                         await enrollments.IsEnrolledAsync(request.Id, facilityId, cancellationToken));

        if (beneficiary is null || !isInScope)
        {
            // Out-of-scope records look like missing ones (same rule as the other features).
            throw new NotFoundException(nameof(BeneficiaryEntity), request.Id);
        }

        // Deactivate instead of deleting: printed codes, match history and the sequence stay valid.
        beneficiary.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
