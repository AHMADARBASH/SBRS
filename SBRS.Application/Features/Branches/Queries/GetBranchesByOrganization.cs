using MediatR;
using SBRS.Application.Features.Branches.DTOs;
using SBRS.Application.Features.Branches.Mappings;
using SBRS.Domain.Interfaces;



namespace SBRS.Application.Features.Branches.Queries.GetBranchesByOrganization;

public record GetBranchesByOrganizationQuery(Guid OrganizationId) : IRequest<List<BranchDto>>;


public class GetBranchesByOrganizationQueryHandler(IBranchRepository branches)
    : IRequestHandler<GetBranchesByOrganizationQuery, List<BranchDto>>
{
    public async Task<List<BranchDto>> Handle(GetBranchesByOrganizationQuery request, CancellationToken ct)
    {
        var entities = await branches.GetByOrganizationIdAsync(request.OrganizationId, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }
}