using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class FacilityPartnerConfiguration : IEntityTypeConfiguration<FacilityPartnerEntity>
{
    public void Configure(EntityTypeBuilder<FacilityPartnerEntity> builder)
    {
        builder.ToTable("FacilitiesPartners");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Facility)
               .WithMany()
               .HasForeignKey(x => x.FacilityId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Partner)
               .WithMany()
               .HasForeignKey(x => x.PartnerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.FacilityId);
        builder.HasIndex(x => x.PartnerId);
    }
}