using MediatR;
using SBRS.Domain.Constants;

namespace SBRS.Application.Features.Users.Queries;

/// <summary>The required and optional ids a role expects when a user is created.</summary>
public sealed record RoleDto(
    string Role,
    bool RequiresOrganizationId,
    bool RequiresFacilityId);

/// <summary>
/// The roles that exist in the system. These are the only values CreateUserCommand
/// accepts, so the caller can use a returned Role as-is when creating a user.
/// </summary>
public sealed record GetRolesQuery : IRequest<IReadOnlyList<RoleDto>>;

public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleDto>>
{
    public Task<IReadOnlyList<RoleDto>> Handle(GetRolesQuery request, CancellationToken ct)
    {
        IReadOnlyList<RoleDto> roles = Roles.All.Select(r => new RoleDto(
            r,
            r == Roles.OrganizationAdmin,
            r == Roles.FacilityAdmin || r == Roles.DataEntry)).ToList();

        return Task.FromResult(roles);
    }
}
