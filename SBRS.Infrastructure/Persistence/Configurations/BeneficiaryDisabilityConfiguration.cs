using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class BeneficiaryDisabilityConfiguration : IEntityTypeConfiguration<BeneficiaryDisabilityEntity>
{
    public void Configure(EntityTypeBuilder<BeneficiaryDisabilityEntity> builder)
    {
        // The table is created new by the migration, so no legacy rows exist that need a
        // nullable reason; the domain and the API already require ReasonId on every row.

        // A beneficiary cannot report the same disability type twice.
        builder.HasIndex(x => new { x.BeneficiaryId, x.DisabilityTypeId }).IsUnique();
        builder.HasIndex(x => x.DisabilityTypeId);

        builder.HasOne<BeneficiaryEntity>().WithMany()
            .HasForeignKey(x => x.BeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DisabilityTypeEntity>().WithMany()
            .HasForeignKey(x => x.DisabilityTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<DisabilityReasonEntity>().WithMany()
            .HasForeignKey(x => x.ReasonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
