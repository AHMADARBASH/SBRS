using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class NationalityConfiguration : IEntityTypeConfiguration<NationalityEntity>
{
    public void Configure(EntityTypeBuilder<NationalityEntity> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.ArName).IsRequired().HasMaxLength(100);
        builder.Property(n => n.EnName).IsRequired().HasMaxLength(100);
        builder.HasIndex(n => n.EnName).IsUnique();
    }
}
