using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;
using SBRS.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserConfiguration :IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(u => u.IsActive)
                .IsRequired();
            builder.Property(u => u.IsMobile)
                .IsRequired();

            builder.HasOne<OrganizationEntity>()
              .WithMany()
              .HasForeignKey(x => x.OrganizationId)
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.OrganizationId);
            builder.HasIndex(x => x.FacilityId);
        }
    }
}
