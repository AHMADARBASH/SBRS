using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class DiseaseCategoryConfiguration : IEntityTypeConfiguration<DiseaseCategoryEntity>
{
    public void Configure(EntityTypeBuilder<DiseaseCategoryEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ArName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EnName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
