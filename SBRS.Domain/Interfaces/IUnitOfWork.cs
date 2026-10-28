using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBRS.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        /// <summary>Begins an explicit database transaction (e.g. serializing concurrent code generation).</summary>
        Task BeginTransactionAsync(CancellationToken ct = default);

        /// <summary>Commits the transaction started by BeginTransactionAsync.</summary>
        Task CommitAsync(CancellationToken ct = default);

        /// <summary>Rolls back the transaction started by BeginTransactionAsync.</summary>
        Task RollbackAsync(CancellationToken ct = default);
    }
}
