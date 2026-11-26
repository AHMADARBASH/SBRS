using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Visits.Commands;

public sealed record DeleteVisitCommand(Guid Id, Guid FacilityId) : IRequest;

public sealed class DeleteVisitCommandValidator : AbstractValidator<DeleteVisitCommand>
{
    public DeleteVisitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
    }
}

public sealed class DeleteVisitCommandHandler(
    IVisitRepository visits,
    IBeneficiaryConsultationRepository consultations,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteVisitCommand>
{
    public async Task Handle(DeleteVisitCommand c, CancellationToken ct)
    {
        var visit = await visits.GetByIdAsync(c.Id, c.FacilityId, ct)
                    ?? throw new NotFoundException(nameof(VisitEntity), c.Id);

        visit.Deactivate();
        foreach (var consultation in await consultations.GetActiveByVisitIdAsync(c.Id, ct))
            consultation.Deactivate();
        foreach (var detail in await consultations.GetActiveDetailsByVisitIdAsync(c.Id, ct))
            detail.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);
    }
}
