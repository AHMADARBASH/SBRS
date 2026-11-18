using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Commands;

/// <summary>
/// Enrolls an existing, globally registered beneficiary at the caller's facility.
/// The facility always comes from the current user; the route only carries the beneficiary id.
/// </summary>
public sealed record EnrollBeneficiaryHereCommand(Guid BeneficiaryId) : IRequest;

public sealed class EnrollBeneficiaryHereCommandValidator : AbstractValidator<EnrollBeneficiaryHereCommand>
{
    public EnrollBeneficiaryHereCommandValidator()
    {
        RuleFor(x => x.BeneficiaryId).NotEmpty();
    }
}

public sealed class EnrollBeneficiaryHereCommandHandler(
    IBeneficiaryRepository beneficiaries,
    IFacilityBeneficiaryRepository enrollments,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<EnrollBeneficiaryHereCommand>
{
    public async Task Handle(EnrollBeneficiaryHereCommand request, CancellationToken cancellationToken)
    {
        var facilityId = currentUser.FacilityId
            ?? throw new ForbiddenException("No facility is assigned to this user.");

        var beneficiary = await beneficiaries.GetByIdAsync(request.BeneficiaryId, cancellationToken);

        // A merged duplicate is not a person any more: the caller must use the survivor.
        if (beneficiary is null ||
            beneficiary.MatchReviewStatus == BeneficiaryMatchReviewStatus.MergedIntoExisting)
            throw new NotFoundException(nameof(BeneficiaryEntity), request.BeneficiaryId);

        var existing = await enrollments.GetAsync(beneficiary.Id, facilityId, cancellationToken);

        if (existing is { IsActive: true })
            throw new ConflictException("The beneficiary is already enrolled at this facility.");

        if (existing is not null)
        {
            // Keep the original EnrolledOn: reactivation is not a new enrollment.
            existing.Activate();
        }
        else
        {
            var enrollment = new FacilityBeneficiaryEntity(
                beneficiary.Id, facilityId, DateOnly.FromDateTime(DateTime.UtcNow));
            await enrollments.AddAsync(enrollment, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
