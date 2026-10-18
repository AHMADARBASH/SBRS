using MediatR;
using SBRS.Application.Features.Facilities.DTOs;
using SBRS.Application.Features.Facilities.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Facilities.Queries;

public record GetFacilitiesByBranchQuery(Guid BranchId, bool IncludeInactive = false)
    : IRequest<List<FacilityDto>>;

public class GetFacilitiesByBranchQueryHandler(IFacilityRepository facilities)
    : IRequestHandler<GetFacilitiesByBranchQuery, List<FacilityDto>>
{
    public async Task<List<FacilityDto>> Handle(GetFacilitiesByBranchQuery request, CancellationToken ct)
    {
        var items = await facilities.GetByBranchAsync(request.BranchId, request.IncludeInactive, ct);
        return items.Select(f => f.ToDto()).ToList();
    }
}