using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class BeneficiaryIdentityConfiguration : IEntityTypeConfiguration<BeneficiaryIdentityEntity>
{
    public void Configure(EntityTypeBuilder<BeneficiaryIdentityEntity> builder)
    {
        builder.ToTable("BeneficiaryIdentities");

        // Empty string is a valid value (UNREGISTERED), so the column is NOT NULL but may be "".
        builder.Property(x => x.Value).IsRequired().HasMaxLength(30);

        builder.HasIndex(x => new { x.BeneficiaryId, x.IdentityTypeId }).IsUnique(); // one per type per beneficiary
        builder.HasIndex(x => new { x.IdentityTypeId, x.Value });                    // lookup by document number

        builder.HasOne<BeneficiaryEntity>().WithMany()
            .HasForeignKey(x => x.BeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IdentityType).WithMany()
            .HasForeignKey(x => x.IdentityTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
