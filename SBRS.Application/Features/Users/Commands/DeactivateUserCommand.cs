using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Constants;
using SBRS.Domain.Exceptions;

namespace SBRS.Application.Features.Users.Commands;

public sealed record DeactivateUserCommand(Guid Id) : IRequest;

public sealed class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class DeactivateUserCommandHandler(IUserAdminService users, ICurrentUser currentUser)
    : IRequestHandler<DeactivateUserCommand>
{
    public async Task Handle(DeactivateUserCommand c, CancellationToken ct)
    {
        if (c.Id == currentUser.UserId)
            throw new DomainException("You cannot deactivate your own account.");
        if(currentUser.Role == Roles.SystemAdmin) 
            throw new DomainException("System Admin cannot be deactivated.");

        await users.DeactivateAsync(c.Id, ct);
    }
}