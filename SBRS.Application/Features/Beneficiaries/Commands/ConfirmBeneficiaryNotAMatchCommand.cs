using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Commands;

public sealed record ConfirmBeneficiaryNotAMatchCommand(Guid Id, string? Reason) : IRequest;

public sealed class ConfirmBeneficiaryNotAMatchCommandValidator : AbstractValidator<ConfirmBeneficiaryNotAMatchCommand>
{
    public ConfirmBeneficiaryNotAMatchCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public sealed class ConfirmBeneficiaryNotAMatchCommandHandler(
    IBeneficiaryRepository repository, ICurrentUser currentUser, IUnitOfWork unitOfWork)
    : IRequestHandler<ConfirmBeneficiaryNotAMatchCommand>
{
    public async Task Handle(ConfirmBeneficiaryNotAMatchCommand request, CancellationToken cancellationToken)
    {
        var beneficiary = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (beneficiary is null ||
            (!currentUser.IsSystemAdmin && beneficiary.RegistrationFacilityId != currentUser.FacilityId))
        {
            // Out-of-scope records look like missing ones (same rule as the other features).
            throw new NotFoundException(nameof(BeneficiaryEntity), request.Id);
        }

        beneficiary.ConfirmNotAMatch(request.Reason, currentUser.UserId!.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
