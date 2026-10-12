using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;
using System.Diagnostics.Metrics;
using System.Net;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class CountryConfiguration : LookupConfiguration<CountryEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CountryEntity> b)
    {
        b.ToTable("Countries");
        b.HasIndex(x => x.Code).IsUnique();
    }
}

public class CityConfiguration : LookupConfiguration<CityEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CityEntity> b)
    {
        b.ToTable("Cities");
        b.HasOne<CountryEntity>().WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.CountryId, x.Code }).IsUnique();
    }
}

public class RegionConfiguration : LookupConfiguration<RegionEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RegionEntity> b)
    {
        b.ToTable("Regions");
        b.HasOne<CityEntity>().WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.CityId, x.Code }).IsUnique();
    }
}

public class DistrictConfiguration : LookupConfiguration<DistrictEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<DistrictEntity> b)
    {
        b.ToTable("Districts");
        b.HasOne<RegionEntity>().WithMany().HasForeignKey(x => x.RegionId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.RegionId, x.Code }).IsUnique();
    }
}

public class AreaConfiguration : LookupConfiguration<AreaEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<AreaEntity> b)
    {
        b.ToTable("Areas");
        b.HasOne<DistrictEntity>().WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.DistrictId, x.Code }).IsUnique();
    }
}
