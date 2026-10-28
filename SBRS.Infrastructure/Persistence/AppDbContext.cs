using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;
using SBRS.Infrastructure.Identity;
using System.Net;


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
        public DbSet<FacilityEntity> Facilities => Set<FacilityEntity>();
        public DbSet<AddressEntity> Addresses => Set<AddressEntity>();
        public DbSet<FacilityTypeEntity> FacilityTypes => Set<FacilityTypeEntity>();
        public DbSet<NationalityEntity> Nationalities => Set<NationalityEntity>();
        public DbSet<IdentityTypeEntity> IdentityTypes => Set<IdentityTypeEntity>();
        public DbSet<AccommodationTypeEntity> AccommodationTypes => Set<AccommodationTypeEntity>();
        public DbSet<DisabilityTypeEntity> DisabilityTypes => Set<DisabilityTypeEntity>();
        public DbSet<EducationTypeEntity> EducationTypes => Set<EducationTypeEntity>();
        public DbSet<PartnerEntity> Partners => Set<PartnerEntity>();
        public DbSet<FacilityPartnerEntity> FacilitiesPartners => Set<FacilityPartnerEntity>();
        public DbSet<BeneficiaryEntity> Beneficiaries => Set<BeneficiaryEntity>();
        public DbSet<BeneficiaryIdentityEntity> BeneficiaryIdentities => Set<BeneficiaryIdentityEntity>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.HasPostgresExtension("citext");
        }

        public Task BeginTransactionAsync(CancellationToken ct = default)
            => Database.BeginTransactionAsync(ct);

        public Task CommitAsync(CancellationToken ct = default)
            => Database.CommitTransactionAsync(ct);

        public Task RollbackAsync(CancellationToken ct = default)
            => Database.RollbackTransactionAsync(ct);

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Property(e => e.CreatedAt).CurrentValue = now;
                else if (entry.State == EntityState.Modified)
                    entry.Property(e => e.UpdatedAt).CurrentValue = now;
            }

            try
            {
                return await base.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // translate the provider-specific unique violation so the Application layer
                // can catch UniqueViolationException without referencing Npgsql
                throw new UniqueViolationException("A database unique constraint was violated.", ex);
            }
        }
    }
}
