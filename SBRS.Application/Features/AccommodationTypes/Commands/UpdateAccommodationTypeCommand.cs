using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.AccommodationTypes.DTOs;
using SBRS.Application.Features.AccommodationTypes.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.AccommodationTypes.Commands;

public sealed record UpdateAccommodationTypeCommand(Guid Id, string ArName, string EnName)
    : IRequest<AccommodationTypeDto>;

public sealed class UpdateAccommodationTypeCommandValidator : AbstractValidator<UpdateAccommodationTypeCommand>
{
    public UpdateAccommodationTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateAccommodationTypeCommandHandler(IAccommodationTypeRepository accommodationTypes, IUnitOfWork uow)
    : IRequestHandler<UpdateAccommodationTypeCommand, AccommodationTypeDto>
{
    public async Task<AccommodationTypeDto> Handle(UpdateAccommodationTypeCommand c, CancellationToken ct)
    {
        var accommodationType = await accommodationTypes.GetByIdAsync(c.Id, ct)
                                ?? throw new NotFoundException(nameof(AccommodationTypeEntity), c.Id);

        var enName = c.EnName.Trim();
        if (await accommodationTypes.EnNameExistsAsync(enName, c.Id, ct))
            throw new ConflictException($"Accommodation type '{enName}' already exists.");

        accommodationType.Update(c.ArName, enName);
        await uow.SaveChangesAsync(ct);

        return accommodationType.ToDto();
    }
}
