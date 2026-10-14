using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Exceptions;

namespace SBRS.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken) : IRequest;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(200);
    }
}

public class LogoutCommandHandler(IRefreshTokenService refreshTokens) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand request, CancellationToken ct)
        => refreshTokens.RevokeAsync(request.RefreshToken, ct);
}

public record LogoutAllCommand : IRequest;

public class LogoutAllCommandHandler(ICurrentUser currentUser, IRefreshTokenService refreshTokens)
    : IRequestHandler<LogoutAllCommand>
{
    public async Task Handle(LogoutAllCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("Unauthorized");
        await refreshTokens.RevokeAllForUserAsync(userId, ct);
    }
}