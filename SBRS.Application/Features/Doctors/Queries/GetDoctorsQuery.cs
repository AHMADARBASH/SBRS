using MediatR;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Doctors.DTOs;
using SBRS.Application.Features.Doctors.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Doctors.Queries;

public sealed record GetDoctorsQuery(Guid? FacilityId, Guid? ClinicId, bool IncludeInactive)
    : IRequest<IReadOnlyList<DoctorDto>>;

public sealed class GetDoctorsQueryHandler(IDoctorRepository doctors, ICurrentUser currentUser)
    : IRequestHandler<GetDoctorsQuery, IReadOnlyList<DoctorDto>>
{
    public async Task<IReadOnlyList<DoctorDto>> Handle(GetDoctorsQuery q, CancellationToken ct)
    {
        var facilityId = currentUser.IsSystemAdmin ? q.FacilityId : currentUser.FacilityId;

        var items = await doctors.GetAllAsync(facilityId, q.ClinicId, q.IncludeInactive, ct);
        return items.Select(d => d.ToDto()).ToList();
    }
}
