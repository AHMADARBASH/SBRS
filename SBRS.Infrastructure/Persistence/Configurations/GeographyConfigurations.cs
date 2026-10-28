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
        b.HasIndex(x => x.Code).IsUnique();
    }
}

public class CityConfiguration : LookupConfiguration<CityEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CityEntity> b)
    {
        b.HasOne<CountryEntity>().WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.CountryId, x.Code }).IsUnique();

        // c_code is the 3-letter city code used to build facility codes (e.g. DAM), unique per system
        b.Property(x => x.C_Code).HasColumnName("c_code").IsRequired().HasMaxLength(3);
        b.HasIndex(x => x.C_Code).IsUnique();
    }
}

public class RegionConfiguration : LookupConfiguration<RegionEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RegionEntity> b)
    {
        b.HasOne<CityEntity>().WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.CityId, x.Code }).IsUnique();
    }
}

public class DistrictConfiguration : LookupConfiguration<DistrictEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<DistrictEntity> b)
    {
        b.HasOne<RegionEntity>().WithMany().HasForeignKey(x => x.RegionId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.RegionId, x.Code }).IsUnique();
    }
}

public class AreaConfiguration : LookupConfiguration<AreaEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<AreaEntity> b)
    {
        b.HasOne<DistrictEntity>().WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<CityEntity>().WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.DistrictId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.CityId, x.ArName }); // list areas of a city, ordered by name
    }
}
