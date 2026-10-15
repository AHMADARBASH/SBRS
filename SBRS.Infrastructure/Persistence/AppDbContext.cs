using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;
using SBRS.Infrastructure.Identity;


namespace SBRS.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
    {
        public DbSet<OrganizationEntity> Organizations => Set<OrganizationEntity>();
        public DbSet<CountryEntity> Countries => Set<CountryEntity>();
        public DbSet<CityEntity> Cities => Set<CityEntity>();
        public DbSet<RegionEntity> Regions => Set<RegionEntity>();
        public DbSet<DistrictEntity> Districts => Set<DistrictEntity>();
        public DbSet<AreaEntity> Areas => Set<AreaEntity>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<BranchEntity> Branches => Set<BranchEntity>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

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
