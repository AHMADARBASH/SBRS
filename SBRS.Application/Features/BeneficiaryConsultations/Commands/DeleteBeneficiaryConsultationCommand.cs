using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryConsultations.Commands;

public sealed record DeleteBeneficiaryConsultationCommand(Guid Id, Guid FacilityId) : IRequest;

public sealed class DeleteBeneficiaryConsultationCommandValidator
    : AbstractValidator<DeleteBeneficiaryConsultationCommand>
{
    public DeleteBeneficiaryConsultationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
    }
}

public sealed class DeleteBeneficiaryConsultationCommandHandler(
    IBeneficiaryConsultationRepository consultations, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBeneficiaryConsultationCommand>
{
    public async Task Handle(DeleteBeneficiaryConsultationCommand c, CancellationToken ct)
    {
        var consultation = await consultations.GetByIdAsync(c.Id, c.FacilityId, ct)
                           ?? throw new NotFoundException(nameof(BeneficiaryConsultationEntity), c.Id);

        consultation.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
