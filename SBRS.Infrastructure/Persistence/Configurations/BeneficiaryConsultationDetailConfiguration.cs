using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class BeneficiaryConsultationDetailConfiguration
    : IEntityTypeConfiguration<BeneficiaryConsultationDetailEntity>
{
    public void Configure(EntityTypeBuilder<BeneficiaryConsultationDetailEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BeneficiaryConsultationId).IsRequired();
        builder.Property(x => x.DiseaseId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.BeneficiaryConsultation).WithMany(c => c.ConsultationDetails)
            .HasForeignKey(x => x.BeneficiaryConsultationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Disease).WithMany()
            .HasForeignKey(x => x.DiseaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.BeneficiaryConsultationId, x.DiseaseId }).IsUnique();
        builder.HasIndex(x => x.DiseaseId);
    }
}
