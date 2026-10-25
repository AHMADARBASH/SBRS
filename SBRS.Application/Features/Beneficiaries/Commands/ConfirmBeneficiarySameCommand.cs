using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.Services;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Commands;

public sealed record ConfirmBeneficiarySameCommand(Guid Id, Guid ExistingBeneficiaryId) : IRequest;

public sealed class ConfirmBeneficiarySameCommandValidator : AbstractValidator<ConfirmBeneficiarySameCommand>
{
    public ConfirmBeneficiarySameCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ExistingBeneficiaryId).NotEmpty().NotEqual(x => x.Id);
    }
}

public sealed class ConfirmBeneficiarySameCommandHandler(
    IBeneficiaryRepository repository, IBeneficiaryMatchService matchService,
    ICurrentUser currentUser, IUnitOfWork unitOfWork) : IRequestHandler<ConfirmBeneficiarySameCommand>
{
    public async Task Handle(ConfirmBeneficiarySameCommand request, CancellationToken cancellationToken)
    {
        var beneficiary = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (beneficiary is null ||
            (!currentUser.IsSystemAdmin && beneficiary.RegistrationFacilityId != currentUser.FacilityId))
        {
            // Out-of-scope records look like missing ones (same rule as the other features).
            throw new NotFoundException(nameof(BeneficiaryEntity), request.Id);
        }

        if (beneficiary.MatchReviewStatus != BeneficiaryMatchReviewStatus.PendingReview)
            throw new DomainException("This beneficiary has no pending match review.");

        // The survivor must be one of the system's own candidates for this case.
        var key = BeneficiaryIdentityKey.From(beneficiary.FirstName, beneficiary.FatherName,
            beneficiary.LastName, beneficiary.MotherName, beneficiary.BirthDate);
        var result = await matchService.FindAsync(key, beneficiary.IsBirthDateEstimated, beneficiary.Id, cancellationToken);

        if (!result.Matches.Any(m => m.Beneficiary.Id == request.ExistingBeneficiaryId))
            throw new DomainException("The selected beneficiary is not one of the match candidates.");

        beneficiary.MergeInto(request.ExistingBeneficiaryId, currentUser.UserId!.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
