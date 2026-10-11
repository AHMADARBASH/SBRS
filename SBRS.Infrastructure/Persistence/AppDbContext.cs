using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) :DbContext(options), IUnitOfWork
    {
        public DbSet<OrganizationEntity> Organizations => Set<OrganizationEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Property(e => e.CreatedAt).CurrentValue = now;
                else if (entry.State == EntityState.Modified)
                    entry.Property(e => e.UpdatedAt).CurrentValue = now;
            }

            return base.SaveChangesAsync(ct);
        }
    }
}
