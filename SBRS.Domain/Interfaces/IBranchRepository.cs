using SBRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Domain.Interfaces
{
    public interface IBranchRepository
    {
        Task<BranchEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<BranchEntity>> GetAllAsync( CancellationToken ct = default);
        Task<List<BranchEntity>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken ct = default);
        Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct = default);
        Task AddAsync(BranchEntity branch, CancellationToken ct = default);
    }
}
