using MediatR;
using SBRS.Application.Features.Branches.DTOs;
using SBRS.Application.Features.Branches.Mappings;
using SBRS.Application.Features.Branches.Queries;
using SBRS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Features.Branches.Queries
{
    public record GetBranchesQuery() : IRequest<List<BranchDto>>;

    public class GetBranchesQueryHandler(IBranchRepository branches) : IRequestHandler<GetBranchesQuery, List<BranchDto>>
    {

        public async Task<List<BranchDto>> Handle(GetBranchesQuery request, CancellationToken ct)
        {
            var entities = await branches.GetAllAsync(ct);
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}