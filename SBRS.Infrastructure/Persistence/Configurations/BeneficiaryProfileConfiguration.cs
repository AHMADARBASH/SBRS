using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class BeneficiaryProfileConfiguration : IEntityTypeConfiguration<BeneficiaryProfileEntity>
{
    public void Configure(EntityTypeBuilder<BeneficiaryProfileEntity> builder)
    {
        // explicit snake_case name: an explicitly configured table name is not rewritten
        // by the naming convention, so the profile must be spelled like the other tables
        builder.ToTable("beneficiary_profiles", t =>
        {
            // Ranges only; the cross-table rules (marital status, gender, dates) live in the handler.
            t.HasCheckConstraint("ck_beneficiary_profiles_wives_number_range",
                "wives_number IS NULL OR wives_number BETWEEN 1 AND 4");
            t.HasCheckConstraint("ck_beneficiary_profiles_children_number_range",
                "children_number IS NULL OR children_number BETWEEN 0 AND 30");
            t.HasCheckConstraint("ck_beneficiary_profiles_displacement_consistency",
                "(is_displaced = true) OR (displacement_date IS NULL AND old_area_id IS NULL)");
        });

        // 1:1 with the beneficiary; the profile is created once and then replaced, never moved.
        builder.HasIndex(x => x.BeneficiaryId).IsUnique();

        builder.Property(x => x.PhoneNumber1).HasMaxLength(20);
        builder.Property(x => x.PhoneNumber2).HasMaxLength(20);
        builder.Property(x => x.CurrentWork).HasMaxLength(200);
        builder.Property(x => x.PreviousWork).HasMaxLength(200);
        // short maps to smallint on PostgreSQL; nullable in the database, rules decide when it may be set.
        builder.Property(x => x.WivesNumber);
        builder.Property(x => x.ChildrenNumber);

        builder.HasOne<BeneficiaryEntity>().WithMany()
            .HasForeignKey(x => x.BeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AreaEntity>().WithMany()
            .HasForeignKey(x => x.OldAreaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.OldAreaId);

        builder.HasOne<AccommodationTypeEntity>().WithMany()
            .HasForeignKey(x => x.AccommodationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BeneficiaryStatusEntity>().WithMany()
            .HasForeignKey(x => x.BeneficiaryStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EducationTypeEntity>().WithMany()
            .HasForeignKey(x => x.EducationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EducationIssueEntity>().WithMany()
            .HasForeignKey(x => x.EducationIssueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MaritalStatusEntity>().WithMany()
            .HasForeignKey(x => x.MaritalStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
