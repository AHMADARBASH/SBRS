using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class FacilityTypeConfiguration : IEntityTypeConfiguration<FacilityTypeEntity>
{
    public void Configure(EntityTypeBuilder<FacilityTypeEntity> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ArName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.EnName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(10);
        builder.HasIndex(t => t.Code).IsUnique();
    }
}