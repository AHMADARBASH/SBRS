using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Doctors.DTOs;
using SBRS.Application.Features.Doctors.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Doctors.Queries;

public sealed record GetDoctorByIdQuery(Guid Id) : IRequest<DoctorDto>;

public sealed class GetDoctorByIdQueryHandler(IDoctorRepository doctors)
    : IRequestHandler<GetDoctorByIdQuery, DoctorDto>
{
    public async Task<DoctorDto> Handle(GetDoctorByIdQuery q, CancellationToken ct)
    {
        var doctor = await doctors.GetByIdWithClinicsAsync(q.Id, ct)
                     ?? throw new NotFoundException(nameof(DoctorEntity), q.Id);

        return doctor.ToDto();
    }
}
