using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class BeneficiaryChronicDiseaseConfiguration : IEntityTypeConfiguration<BeneficiaryChronicDiseaseEntity>
{
    public void Configure(EntityTypeBuilder<BeneficiaryChronicDiseaseEntity> builder)
    {
        builder.HasIndex(x => new { x.BeneficiaryId, x.ChronicDiseaseId }).IsUnique();
        builder.HasIndex(x => x.ChronicDiseaseId);

        builder.HasOne<BeneficiaryEntity>().WithMany()
            .HasForeignKey(x => x.BeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ChronicDiseaseEntity>().WithMany()
            .HasForeignKey(x => x.ChronicDiseaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
