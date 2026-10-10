using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
