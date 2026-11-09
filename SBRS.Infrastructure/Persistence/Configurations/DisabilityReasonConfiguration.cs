using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class DisabilityReasonConfiguration : IEntityTypeConfiguration<DisabilityReasonEntity>
{
    public void Configure(EntityTypeBuilder<DisabilityReasonEntity> builder)
    {
        builder.Property(x => x.ArName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EnName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.EnName).IsUnique();
    }
}
