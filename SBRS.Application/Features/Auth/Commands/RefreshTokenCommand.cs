using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Auth.DTOs;
using SBRS.Application.Features.Auth.Mappings;
using SBRS.Application.Common.Exceptions;

namespace SBRS.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResultDto>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(200);
    }
}

public class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokens,
    IIdentityService identityService,
    ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, LoginResultDto>
{
    public async Task<LoginResultDto> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var rotation = await refreshTokens.RotateAsync(request.RefreshToken, ct);

        var user = await identityService.GetAuthenticatedUserAsync(rotation.UserId, ct);
        if (user is null)
        {
            await refreshTokens.RevokeAllForUserAsync(rotation.UserId, ct);
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var access = tokenService.CreateToken(user);
        return user.ToLoginResult(access, rotation.RefreshToken);
    }
}