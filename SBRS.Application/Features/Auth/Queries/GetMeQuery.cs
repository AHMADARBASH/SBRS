using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Auth.DTOs;

namespace SBRS.Application.Features.Auth.Queries;

/// <summary>
/// One call for the client shell after login: the full user object plus the full
/// facility/organization context. Login and Refresh return the same payload (plus the tokens).
/// </summary>
public sealed record GetMeQuery : IRequest<CurrentUserDto>;

public sealed class GetMeQueryHandler(
    IIdentityService identity,
    IUserSessionService session,
    ICurrentUser currentUser)
    : IRequestHandler<GetMeQuery, CurrentUserDto>
{
    public async Task<CurrentUserDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedException("No authenticated user.");

        var user = await identity.GetAuthenticatedUserAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException("The user no longer exists.");

        return await session.BuildAsync(user, cancellationToken);
    }
}
