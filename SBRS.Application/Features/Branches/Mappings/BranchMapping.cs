using SBRS.Application.Features.Branches.DTOs;
using SBRS.Domain.Entities;


namespace SBRS.Application.Features.Branches.Mappings
{
    public static class BranchMapping
    {
        public static BranchDto ToDto(this BranchEntity b)
            => new(b.Id, b.ArName, b.EnName,b.organizationId, b.IsActive, b.CreatedAt, b.UpdatedAt);
    }
}
