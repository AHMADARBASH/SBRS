using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Auth.DTOs;
using SBRS.Application.Features.Auth.Mappings;
using SBRS.Application.Common.Exceptions;

namespace SBRS.Application.Features.Auth.Commands;

public record LoginCommand(string UserName, string Password) : IRequest<LoginResultDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
    }
}

public class LoginCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokens,
    IUserSessionService session)
    : IRequestHandler<LoginCommand, LoginResultDto>
{
    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await identityService.ValidateCredentialsAsync(request.UserName.Trim(), request.Password, ct)
                   ?? throw new UnauthorizedException("Unauthorized");

        var access = tokenService.CreateToken(user);
        var refresh = await refreshTokens.IssueAsync(user.Id, ct);

        // Same payload as Me: the client shell does not need a second call after login.
        var currentUser = await session.BuildAsync(user, ct);

        return currentUser.ToLoginResult(access, refresh);
    }
}