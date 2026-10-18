using MediatR;
using SBRS.Application.Features.Branches.DTOs;
using SBRS.Application.Features.Branches.Mappings;
using SBRS.Domain.Interfaces;



namespace SBRS.Application.Features.Branches.Queries.GetBranchesByOrganization;

public record GetBranchesByOrganizationIdQuery(Guid OrganizationId) : IRequest<List<BranchDto>>;


public class GetBranchesByOrganizationIdQueryHandler(IBranchRepository branches)
    : IRequestHandler<GetBranchesByOrganizationIdQuery, List<BranchDto>>
{
    public async Task<List<BranchDto>> Handle(GetBranchesByOrganizationIdQuery request, CancellationToken ct)
    {
        var entities = await branches.GetByOrganizationIdAsync(request.OrganizationId, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }
}