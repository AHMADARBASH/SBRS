using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class FacilityConfiguration : IEntityTypeConfiguration<FacilityEntity>
{
    public void Configure(EntityTypeBuilder<FacilityEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ArName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EnName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(10).HasColumnType("citext");
        builder.Property(x => x.AddressId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(f => f.TypeId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.PartnerId).IsRequired();

        builder.HasOne(x => x.Branch)
               .WithMany()
               .HasForeignKey(x => x.BranchId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Address)
               .WithMany()
               .HasForeignKey(x => x.AddressId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FacilityType)
                .WithMany()
                .HasForeignKey(x => x.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Partner)
                .WithMany()
                .HasForeignKey(x => x.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.BranchId);
        builder.HasIndex(x => x.AddressId);
        builder.HasIndex(f => f.TypeId);
        builder.HasIndex(x => x.PartnerId);
    }
}