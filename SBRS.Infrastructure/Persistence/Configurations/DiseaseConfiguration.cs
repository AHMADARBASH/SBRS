using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class DiseaseConfiguration : IEntityTypeConfiguration<DiseaseEntity>
{
    public void Configure(EntityTypeBuilder<DiseaseEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ArName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EnName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.DiseaseCategoryId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.DiseaseCategory).WithMany()
            .HasForeignKey(x => x.DiseaseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.DiseaseCategoryId);
    }
}
