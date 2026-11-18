using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class FacilityBeneficiaryConfiguration : IEntityTypeConfiguration<FacilityBeneficiaryEntity>
{
    public void Configure(EntityTypeBuilder<FacilityBeneficiaryEntity> builder)
    {
        // explicit snake_case name, like beneficiary_profiles; the naming convention does not
        // rewrite an explicitly configured table name
        builder.ToTable("facility_beneficiaries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BeneficiaryId).IsRequired();
        builder.Property(x => x.FacilityId).IsRequired();
        builder.Property(x => x.EnrolledOn).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        // one enrollment per (facility, beneficiary): the facility list and enroll endpoint
        // never create duplicates, and the merge must respect this index
        builder.HasIndex(x => new { x.FacilityId, x.BeneficiaryId }).IsUnique();
        builder.HasIndex(x => x.BeneficiaryId); // affordable lookup when moving rows on merge

        builder.HasOne<BeneficiaryEntity>().WithMany()
            .HasForeignKey(x => x.BeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<FacilityEntity>().WithMany()
            .HasForeignKey(x => x.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
