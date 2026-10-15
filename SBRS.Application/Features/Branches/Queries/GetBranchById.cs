using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Branches.DTOs;
using SBRS.Application.Features.Branches.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Features.Branches.Queries
{
    public record GetBranchById(Guid Id) : IRequest<List<BranchDto>>;

    public class GetBranchByIdHandler(IBranchRepository branches) : IRequestHandler<GetBranchById, List<BranchDto>>
    {
        public async Task<List<BranchDto>> Handle(GetBranchById request, CancellationToken ct)
        {
            var entity = await branches.GetByIdAsync(request.Id, ct);
            if (entity == null)
            {
                throw new NotFoundException(nameof(BranchEntity), request.Id);
            }
            return new List<BranchDto> { entity.ToDto() };
        }
    }


}
