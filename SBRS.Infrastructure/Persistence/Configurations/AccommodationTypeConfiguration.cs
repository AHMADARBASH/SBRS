using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class AccommodationTypeConfiguration : IEntityTypeConfiguration<AccommodationTypeEntity>
{
    public void Configure(EntityTypeBuilder<AccommodationTypeEntity> builder)
    {
        builder.ToTable("AccommodationTypes");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ArName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EnName).IsRequired().HasMaxLength(100);
        builder.HasIndex(a => a.EnName).IsUnique();
    }
}
