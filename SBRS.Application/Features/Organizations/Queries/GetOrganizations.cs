using MediatR;
using SBRS.Application.Features.Organizations.DTOs;
using SBRS.Application.Features.Organizations.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Organizations.Queries.GetOrganizations;

public record GetOrganizationsQuery(bool IncludeInactive = false) : IRequest<List<OrganizationDto>>;

public class GetOrganizationsQueryHandler(IOrganizationRepository organizations)
    : IRequestHandler<GetOrganizationsQuery, List<OrganizationDto>>
{
    public async Task<List<OrganizationDto>> Handle(GetOrganizationsQuery request, CancellationToken ct)
    {
        var items = await organizations.GetAllAsync(request.IncludeInactive, ct);
        return items.Select(o => o.ToDto()).ToList();
    }
}