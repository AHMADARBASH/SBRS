using SBRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Domain.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<OrganizationEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<OrganizationEntity>> GetAllAsync(bool includeInactive, CancellationToken ct = default);
        Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct = default);
        Task AddAsync(OrganizationEntity organization, CancellationToken ct = default);
    }
}
