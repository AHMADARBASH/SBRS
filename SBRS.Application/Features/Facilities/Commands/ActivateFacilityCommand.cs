using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Facilities.Commands;

public record ActivateFacilityCommand(Guid Id) : IRequest;

public class ActivateFacilityCommandValidator : AbstractValidator<ActivateFacilityCommand>
{
    public ActivateFacilityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class ActivateFacilityCommandHandler(
    IFacilityRepository facilities,
    IBranchRepository branches,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateFacilityCommand>
{
    public async Task Handle(ActivateFacilityCommand request, CancellationToken ct)
    {
        var entity = await facilities.GetByIdAsync(request.Id, ct)
                     ?? throw new NotFoundException(nameof(FacilityEntity), request.Id);

        var branch = await branches.GetByIdAsync(entity.BranchId, ct)
                     ?? throw new NotFoundException(nameof(BranchEntity), entity.BranchId);

        if (!branch.IsActive)
            throw new DomainException("Cannot activate a facility under a deactivated branch.");

        entity.Activate();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
