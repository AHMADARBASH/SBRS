using MediatR;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Clinics.DTOs;
using SBRS.Application.Features.Clinics.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Clinics.Queries;

public sealed record GetClinicsQuery(Guid? FacilityId, bool IncludeInactive)
    : IRequest<IReadOnlyList<ClinicDto>>;

public sealed class GetClinicsQueryHandler(IClinicRepository clinics, ICurrentUser currentUser)
    : IRequestHandler<GetClinicsQuery, IReadOnlyList<ClinicDto>>
{
    public async Task<IReadOnlyList<ClinicDto>> Handle(GetClinicsQuery q, CancellationToken ct)
    {
        // FacilityAdmin and DataEntry always see their own facility; any filter is ignored for them.
        var facilityId = currentUser.IsSystemAdmin ? q.FacilityId : currentUser.FacilityId;

        var items = await clinics.GetAllAsync(facilityId, q.IncludeInactive, ct);
        return items.Select(c => c.ToDto()).ToList();
    }
}
