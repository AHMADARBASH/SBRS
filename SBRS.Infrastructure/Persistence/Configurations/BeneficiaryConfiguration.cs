using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class BeneficiaryConfiguration : IEntityTypeConfiguration<BeneficiaryEntity>
{
    public void Configure(EntityTypeBuilder<BeneficiaryEntity> builder)
    {
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
        builder.Property(x => x.Code).IsRequired();   // int column; NOT database-generated (no identity/sequence)
        builder.Property(x => x.FullCode).IsRequired().HasMaxLength(50).HasColumnType("citext");
        builder.Property(x=> x.Gender).IsRequired();

        builder.HasIndex(x => new { x.MatchKey, x.Disambiguator }).IsUnique();
        // the number is per facility: the same Code may exist in different facilities
        builder.HasIndex(x => new { x.RegistrationFacilityId, x.Code }).IsUnique();
        builder.HasIndex(x => x.FullCode).IsUnique();   // unique across the whole system
        builder.HasIndex(x => new { x.NormalizedLastName, x.BirthDate }); // probable-match search
        builder.HasIndex(x => new { x.NormalizedLastName, x.NormalizedFirstName }); // name search
        builder.HasIndex(x => new { x.RegistrationFacilityId, x.MatchReviewStatus }); // pending list

        builder.HasOne<AreaEntity>().WithMany()
            .HasForeignKey(x => x.AreaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.AreaId); // reports and filters by area

        builder.HasOne<NationalityEntity>().WithMany()
            .HasForeignKey(x => x.NationalityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.NationalityId); // filters and reports by nationality
        builder.HasIndex(x => new { x.RegistrationFacilityId, x.CreatedAt }); // facility list, newest first

        builder.HasOne<FacilityEntity>().WithMany()
            .HasForeignKey(x => x.RegistrationFacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BeneficiaryEntity>().WithMany()
            .HasForeignKey(x => x.MergedIntoBeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
