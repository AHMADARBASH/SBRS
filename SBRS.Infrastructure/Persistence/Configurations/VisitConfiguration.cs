using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<VisitEntity>
{
    public void Configure(EntityTypeBuilder<VisitEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FacilityId).IsRequired();
        builder.Property(x => x.BeneficiaryId).IsRequired();
        builder.Property(x => x.VisitDate).IsRequired();
        builder.Property(x => x.Weight).HasPrecision(5, 2);
        builder.Property(x => x.Height).HasPrecision(5, 2);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<FacilityEntity>().WithMany()
            .HasForeignKey(x => x.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Beneficiary).WithMany()
            .HasForeignKey(x => x.BeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FacilityId, x.BeneficiaryId, x.VisitDate });
        builder.HasIndex(x => x.FacilityId);
        builder.HasIndex(x => x.BeneficiaryId);
    }
}
