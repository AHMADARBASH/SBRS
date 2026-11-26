using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class WorkingAddressConfiguration : IEntityTypeConfiguration<WorkingAddressEntity>
{
    public void Configure(EntityTypeBuilder<WorkingAddressEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FacilityId).IsRequired();
        builder.Property(x => x.AddressId).IsRequired();
        builder.Property(x => x.WorkingDate).IsRequired();
        builder.Property(x => x.TypeId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<FacilityEntity>().WithMany()
            .HasForeignKey(x => x.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Address).WithMany()
            .HasForeignKey(x => x.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Type).WithMany()
            .HasForeignKey(x => x.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FacilityId, x.WorkingDate });
        builder.HasIndex(x => x.AddressId);
        builder.HasIndex(x => x.TypeId);
    }
}
