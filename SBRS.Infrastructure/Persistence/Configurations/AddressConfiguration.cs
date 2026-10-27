using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;
using System.Net;

namespace SBRS.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<AddressEntity>
{
    public void Configure(EntityTypeBuilder<AddressEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ArName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.EnName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<AreaEntity>()
               .WithMany()
               .HasForeignKey(x => x.AreaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.AreaId, x.ArName }).IsUnique();
        builder.HasIndex(x => new { x.AreaId, x.EnName }).IsUnique();
    }
}