using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class EducationTypeConfiguration : IEntityTypeConfiguration<EducationTypeEntity>
{
    public void Configure(EntityTypeBuilder<EducationTypeEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ArName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EnName).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.EnName).IsUnique();
    }
}
