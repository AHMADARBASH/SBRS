using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Doctors.DTOs;
using SBRS.Application.Features.Doctors.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Doctors.Commands;

public sealed record UpdateDoctorCommand(Guid Id, string FirstName, string LastName)
    : IRequest<DoctorDto>;

public sealed class UpdateDoctorCommandValidator : AbstractValidator<UpdateDoctorCommand>
{
    public UpdateDoctorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateDoctorCommandHandler(IDoctorRepository doctors, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateDoctorCommand, DoctorDto>
{
    public async Task<DoctorDto> Handle(UpdateDoctorCommand c, CancellationToken ct)
    {
        var doctor = await doctors.GetByIdAsync(c.Id, ct)
                     ?? throw new NotFoundException(nameof(DoctorEntity), c.Id);

        doctor.Update(c.FirstName, c.LastName);
        await unitOfWork.SaveChangesAsync(ct);

        return (await doctors.GetByIdWithClinicsAsync(c.Id, ct)
                ?? throw new NotFoundException(nameof(DoctorEntity), c.Id)).ToDto();
    }
}
