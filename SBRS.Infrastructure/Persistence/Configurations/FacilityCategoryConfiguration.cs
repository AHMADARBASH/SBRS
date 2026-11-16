using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class FacilityCategoryConfiguration : IEntityTypeConfiguration<FacilityCategoryEntity>
{
    public void Configure(EntityTypeBuilder<FacilityCategoryEntity> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ArName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.EnName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(c => c.EnName).IsUnique();
        builder.HasIndex(c => c.Code).IsUnique();
    }
}
