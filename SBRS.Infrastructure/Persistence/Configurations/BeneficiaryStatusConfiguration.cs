using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class BeneficiaryStatusConfiguration : IEntityTypeConfiguration<BeneficiaryStatusEntity>
{
    public void Configure(EntityTypeBuilder<BeneficiaryStatusEntity> builder)
    {
        builder.Property(x => x.ArName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EnName).IsRequired().HasMaxLength(100);
        // codes are longer than the old 10-char style (NOT_AVAILABLE), so 50 keeps them stable
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.EnName).IsUnique();
    }
}
