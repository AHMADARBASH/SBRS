using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Users.DTOs;
using SBRS.Domain.Constants;

namespace SBRS.Application.Features.Users.Commands;

public sealed record CreateUserCommand(
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    string Role,
    Guid? OrganizationId,
    Guid? FacilityId,
    bool IsMobile) : IRequest<UserDto>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .Length(3, 50)
            .Matches("^[A-Za-z0-9._-]+$")
            .WithMessage("User name may contain only letters, digits, '.', '_' and '-'.");

        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => Roles.All.Contains(r))
            .WithMessage("Unknown role.");

        When(x => x.Role == Roles.OrganizationAdmin, () =>
            RuleFor(x => x.OrganizationId).NotEmpty()
                .WithMessage("OrganizationId is required for OrganizationAdmin."));

        When(x => x.Role == Roles.FacilityAdmin || x.Role == Roles.DataEntry, () =>
            RuleFor(x => x.FacilityId).NotEmpty()
                .WithMessage("FacilityId is required for FacilityAdmin and DataEntry."));
    }
}

public sealed class CreateUserCommandHandler(IUserAdminService users)
    : IRequestHandler<CreateUserCommand, UserDto>
{
    public Task<UserDto> Handle(CreateUserCommand c, CancellationToken ct) =>
        users.CreateAsync(
            c.UserName.Trim(), c.Password, c.FirstName.Trim(), c.LastName.Trim(),
            c.Role, c.OrganizationId, c.FacilityId, c.IsMobile, ct);
}