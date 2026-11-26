using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Clinics.DTOs;
using SBRS.Application.Features.Clinics.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Clinics.Queries;

public sealed record GetClinicByIdQuery(Guid Id) : IRequest<ClinicDto>;

public sealed class GetClinicByIdQueryHandler(IClinicRepository clinics)
    : IRequestHandler<GetClinicByIdQuery, ClinicDto>
{
    public async Task<ClinicDto> Handle(GetClinicByIdQuery q, CancellationToken ct)
    {
        var clinic = await clinics.GetByIdAsync(q.Id, ct)
                     ?? throw new NotFoundException(nameof(ClinicEntity), q.Id);

        return clinic.ToDto();
    }
}
