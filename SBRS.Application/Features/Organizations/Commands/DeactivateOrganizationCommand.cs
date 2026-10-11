using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Organizations.Commands.DeactivateOrganization;

public record DeactivateOrganizationCommand(Guid Id) : IRequest;

public class DeactivateOrganizationCommandValidator : AbstractValidator<DeactivateOrganizationCommand>
{
    public DeactivateOrganizationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeactivateOrganizationCommandHandler(IOrganizationRepository organizations, IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateOrganizationCommand>
{
    public async Task Handle(DeactivateOrganizationCommand request, CancellationToken ct)
    {
        var entity = await organizations.GetByIdAsync(request.Id, ct)
                     ?? throw new NotFoundException(nameof(OrganizationEntity), request.Id);

        entity.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
    }
}