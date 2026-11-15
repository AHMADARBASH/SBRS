using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;

namespace SBRS.Application.Features.Users.Commands;

/// <summary>Re-enables a deactivated user (e.g. returning to work).</summary>
public sealed record ActivateUserCommand(Guid Id) : IRequest;

public sealed class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class ActivateUserCommandHandler(IUserAdminService users)
    : IRequestHandler<ActivateUserCommand>
{
    public Task Handle(ActivateUserCommand c, CancellationToken ct) => users.ActivateAsync(c.Id, ct);
}
