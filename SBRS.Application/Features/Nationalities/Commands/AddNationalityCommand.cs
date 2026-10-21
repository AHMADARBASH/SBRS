using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Nationalities.DTOs;
using SBRS.Application.Features.Nationalities.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Nationalities.Commands;

public sealed record AddNationalityCommand(string ArName, string EnName) : IRequest<NationalityDto>;

public sealed class AddNationalityCommandValidator : AbstractValidator<AddNationalityCommand>
{
    public AddNationalityCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
    }
}

public sealed class AddNationalityCommandHandler(INationalityRepository nationalities, IUnitOfWork uow)
    : IRequestHandler<AddNationalityCommand, NationalityDto>
{
    public async Task<NationalityDto> Handle(AddNationalityCommand c, CancellationToken ct)
    {
        var enName = c.EnName.Trim();
        if (await nationalities.EnNameExistsAsync(enName, null, ct))
            throw new ConflictException($"Nationality '{enName}' already exists.");

        var nationality = new NationalityEntity(c.ArName, enName);
        await nationalities.AddAsync(nationality, ct);
        await uow.SaveChangesAsync(ct);

        return nationality.ToDto();
    }
}
