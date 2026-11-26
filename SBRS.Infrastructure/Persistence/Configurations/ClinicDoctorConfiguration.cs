using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class ClinicDoctorConfiguration : IEntityTypeConfiguration<ClinicDoctorEntity>
{
    public void Configure(EntityTypeBuilder<ClinicDoctorEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClinicId).IsRequired();
        builder.Property(x => x.DoctorId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor).WithMany(d => d.ClinicDoctors)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ClinicId, x.DoctorId }).IsUnique();
        builder.HasIndex(x => x.DoctorId);
        builder.HasIndex(x => x.ClinicId);
    }
}
