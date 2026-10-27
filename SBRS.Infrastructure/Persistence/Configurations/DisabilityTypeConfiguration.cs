using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class DisabilityTypeConfiguration : IEntityTypeConfiguration<DisabilityTypeEntity>
{
    public void Configure(EntityTypeBuilder<DisabilityTypeEntity> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.ArName).IsRequired().HasMaxLength(100);
        builder.Property(d => d.EnName).IsRequired().HasMaxLength(100);
        builder.HasIndex(d => d.EnName).IsUnique();
    }
}
