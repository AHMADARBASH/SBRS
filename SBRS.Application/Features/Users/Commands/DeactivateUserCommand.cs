using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;
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

        // The target's role is checked in the service, where the user is loaded (the caller is SA by policy).
        await users.DeactivateAsync(c.Id, ct);
    }
}