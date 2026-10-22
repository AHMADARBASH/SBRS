using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class IdentityTypeConfiguration : IEntityTypeConfiguration<IdentityTypeEntity>
{
    public void Configure(EntityTypeBuilder<IdentityTypeEntity> builder)
    {
        builder.ToTable("IdentityTypes");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ArName).IsRequired().HasMaxLength(100);
        builder.Property(i => i.EnName).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Code).IsRequired().HasMaxLength(10);
        builder.HasIndex(i => i.EnName).IsUnique();
        builder.HasIndex(i => i.Code).IsUnique();
    }
}
