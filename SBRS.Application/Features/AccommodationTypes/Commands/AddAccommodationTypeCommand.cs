using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.AccommodationTypes.DTOs;
using SBRS.Application.Features.AccommodationTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.AccommodationTypes.Commands;

public sealed record AddAccommodationTypeCommand(string ArName, string EnName) : IRequest<AccommodationTypeDto>;

public sealed class AddAccommodationTypeCommandValidator : AbstractValidator<AddAccommodationTypeCommand>
{
    public AddAccommodationTypeCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
    }
}

public sealed class AddAccommodationTypeCommandHandler(IAccommodationTypeRepository accommodationTypes, IUnitOfWork uow)
    : IRequestHandler<AddAccommodationTypeCommand, AccommodationTypeDto>
{
    public async Task<AccommodationTypeDto> Handle(AddAccommodationTypeCommand c, CancellationToken ct)
    {
        var enName = c.EnName.Trim();
        if (await accommodationTypes.EnNameExistsAsync(enName, null, ct))
            throw new ConflictException($"Accommodation type '{enName}' already exists.");

        var accommodationType = new AccommodationTypeEntity(c.ArName, enName);
        await accommodationTypes.AddAsync(accommodationType, ct);
        await uow.SaveChangesAsync(ct);

        return accommodationType.ToDto();
    }
}
