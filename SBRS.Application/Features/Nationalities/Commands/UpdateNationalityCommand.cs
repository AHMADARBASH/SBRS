using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Nationalities.DTOs;
using SBRS.Application.Features.Nationalities.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Nationalities.Commands;

public sealed record UpdateNationalityCommand(Guid Id, string ArName, string EnName)
    : IRequest<NationalityDto>;

public sealed class UpdateNationalityCommandValidator : AbstractValidator<UpdateNationalityCommand>
{
    public UpdateNationalityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateNationalityCommandHandler(INationalityRepository nationalities, IUnitOfWork uow)
    : IRequestHandler<UpdateNationalityCommand, NationalityDto>
{
    public async Task<NationalityDto> Handle(UpdateNationalityCommand c, CancellationToken ct)
    {
        var nationality = await nationalities.GetByIdAsync(c.Id, ct)
                          ?? throw new NotFoundException(nameof(NationalityEntity), c.Id);

        var enName = c.EnName.Trim();
        if (await nationalities.EnNameExistsAsync(enName, c.Id, ct))
            throw new ConflictException($"Nationality '{enName}' already exists.");

        nationality.Update(c.ArName, enName);
        await uow.SaveChangesAsync(ct);

        return nationality.ToDto();
    }
}
