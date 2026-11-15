using FluentValidation;
using MediatR;
using SBRS.Application.Common.DTOs;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Models;
using SBRS.Application.Features.Users.DTOs;
using SBRS.Domain.Constants;

namespace SBRS.Application.Features.Users.Queries;

/// <summary>
/// Paged user list for the SystemAdmin screen. <c>search</c> matches the user name,
/// first name or last name; <c>role</c> filters to one role (exact, as returned by
/// <c>GetRolesQuery</c>); inactive users are hidden unless <c>IncludeInactive</c> is set.
/// </summary>
public sealed record GetPagedUsersQuery(
    int Page = 1,
    int PageSize = 20,
    bool IncludeInactive = false,
    string? Search = null,
    string? Role = null) : IRequest<PagedResult<UserListItemDto>>;

public sealed class GetPagedUsersQueryValidator : AbstractValidator<GetPagedUsersQuery>
{
    public GetPagedUsersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(100);

        RuleFor(x => x.Role)
            .Must(r => string.IsNullOrWhiteSpace(r) || Roles.All.Contains(r))
            .WithMessage("Unknown role.");
    }
}

public sealed class GetPagedUsersQueryHandler(IUserAdminService users)
    : IRequestHandler<GetPagedUsersQuery, PagedResult<UserListItemDto>>
{
    public Task<PagedResult<UserListItemDto>> Handle(GetPagedUsersQuery q, CancellationToken ct) =>
        users.GetPagedAsync(q.Page, q.PageSize, q.IncludeInactive, q.Search, q.Role, ct);
}
