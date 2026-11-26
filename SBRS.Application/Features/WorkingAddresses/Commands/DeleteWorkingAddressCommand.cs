using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.WorkingAddresses.Commands;

public sealed record DeleteWorkingAddressCommand(Guid Id, Guid FacilityId) : IRequest;

public sealed class DeleteWorkingAddressCommandValidator : AbstractValidator<DeleteWorkingAddressCommand>
{
    public DeleteWorkingAddressCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
    }
}

public sealed class DeleteWorkingAddressCommandHandler(
    IWorkingAddressRepository workingAddresses, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteWorkingAddressCommand>
{
    public async Task Handle(DeleteWorkingAddressCommand c, CancellationToken ct)
    {
        var entity = await workingAddresses.GetByIdTrackedAsync(c.Id, c.FacilityId, ct)
                     ?? throw new NotFoundException(nameof(WorkingAddressEntity), c.Id);

        entity.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
