using FluentValidation;
using MediatR;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Users.DTOs;
using SBRS.Domain.Constants;
using SBRS.Domain.Exceptions;

namespace SBRS.Application.Features.Users.Commands;

public sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Role,
    Guid? OrganizationId,
    Guid? FacilityId,
    bool IsMobile) : IRequest<UserDto>;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
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

public sealed class UpdateUserCommandHandler(IUserAdminService users, ICurrentUser currentUser)
    : IRequestHandler<UpdateUserCommand, UserDto>
{
    public Task<UserDto> Handle(UpdateUserCommand c, CancellationToken ct)
    {
        if (c.Id == currentUser.UserId && c.Role != currentUser.Role)
            throw new DomainException("You cannot change your own role.");

        return users.UpdateAsync(
            c.Id, c.FirstName.Trim(), c.LastName.Trim(),
            c.Role, c.OrganizationId, c.FacilityId, c.IsMobile, ct);
    }
}