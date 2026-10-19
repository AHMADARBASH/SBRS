using MediatR;
using SBRS.Application.Features.FacilityPartners.DTOs;
using SBRS.Application.Features.FacilityPartners.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.FacilityPartners.Queries;

public sealed record GetFacilityPartnersQuery(Guid FacilityId) : IRequest<IReadOnlyList<FacilityPartnerDto>>;

public sealed class GetFacilityPartnersQueryHandler(IFacilityPartnerRepository history)
    : IRequestHandler<GetFacilityPartnersQuery, IReadOnlyList<FacilityPartnerDto>>
{
    public async Task<IReadOnlyList<FacilityPartnerDto>> Handle(GetFacilityPartnersQuery q, CancellationToken ct) =>
        (await history.GetByFacilityAsync(q.FacilityId, ct)).Select(e => e.ToDto()).ToList();
}