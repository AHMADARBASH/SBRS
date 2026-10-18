using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Facilities.Commands;

public record DeactivateFacilityCommand(Guid Id) : IRequest;

public class DeactivateFacilityCommandValidator : AbstractValidator<DeactivateFacilityCommand>
{
    public DeactivateFacilityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeactivateFacilityCommandHandler(IFacilityRepository facilities, IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateFacilityCommand>
{
    public async Task Handle(DeactivateFacilityCommand request, CancellationToken ct)
    {
        var entity = await facilities.GetByIdAsync(request.Id, ct)
                     ?? throw new NotFoundException(nameof(FacilityEntity), request.Id);

        entity.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
    }
}