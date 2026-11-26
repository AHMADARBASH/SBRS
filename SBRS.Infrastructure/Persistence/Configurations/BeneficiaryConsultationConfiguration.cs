using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class BeneficiaryConsultationConfiguration : IEntityTypeConfiguration<BeneficiaryConsultationEntity>
{
    public void Configure(EntityTypeBuilder<BeneficiaryConsultationEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConsultationDate).IsRequired();
        builder.Property(x => x.VisitId).IsRequired();
        builder.Property(x => x.ClinicId).IsRequired();
        builder.Property(x => x.DoctorId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Visit).WithMany()
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor).WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.VisitId);
        builder.HasIndex(x => x.ClinicId);
        builder.HasIndex(x => x.DoctorId);
    }
}
