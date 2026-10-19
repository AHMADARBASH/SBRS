using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;

namespace SBRS.Application.Features.Users.Commands;

public sealed record ResetUserPasswordCommand(Guid Id, string NewPassword) : IRequest;

public sealed class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}

public sealed class ResetUserPasswordCommandHandler(IUserAdminService users)
    : IRequestHandler<ResetUserPasswordCommand>
{
    public Task Handle(ResetUserPasswordCommand c, CancellationToken ct) =>
        users.ResetPasswordAsync(c.Id, c.NewPassword, ct);
}