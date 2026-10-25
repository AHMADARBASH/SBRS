using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class BeneficiaryConfiguration : IEntityTypeConfiguration<BeneficiaryEntity>
{
    public void Configure(EntityTypeBuilder<BeneficiaryEntity> builder)
    {
        builder.ToTable("Beneficiaries");

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FatherName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.MotherName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.MatchReviewStatus).HasConversion<byte>();
        builder.Property(x => x.EnrollmentDate).IsRequired();

        builder.Property(x => x.NormalizedFirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NormalizedFatherName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NormalizedLastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NormalizedMotherName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.MatchKey).IsRequired().HasMaxLength(64).IsFixedLength().IsUnicode(false);
        builder.Property(x => x.DistinctReason).HasMaxLength(500);
        builder.Property(x => x.FullCode).IsRequired().HasMaxLength(50);

        builder.HasIndex(x => new { x.MatchKey, x.Disambiguator }).IsUnique();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.FullCode).IsUnique();
        builder.HasIndex(x => new { x.NormalizedLastName, x.BirthDate }); // probable-match search
        builder.HasIndex(x => new { x.RegistrationFacilityId, x.MatchReviewStatus }); // pending list

        builder.HasOne<FacilityEntity>().WithMany()
            .HasForeignKey(x => x.RegistrationFacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BeneficiaryEntity>().WithMany()
            .HasForeignKey(x => x.MergedIntoBeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
