using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<ClinicEntity>
{
    public void Configure(EntityTypeBuilder<ClinicEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FacilityId).IsRequired();
        builder.Property(x => x.ClinicTypeId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<FacilityEntity>().WithMany()
            .HasForeignKey(x => x.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClinicType).WithMany()
            .HasForeignKey(x => x.ClinicTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FacilityId, x.ClinicTypeId }).IsUnique();
        builder.HasIndex(x => x.FacilityId);
        builder.HasIndex(x => x.ClinicTypeId);
    }
}
