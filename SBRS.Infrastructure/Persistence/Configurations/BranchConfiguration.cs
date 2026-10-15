using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Infrastructure.Persistence.Configurations
{
    public class BranchConfiguration : IEntityTypeConfiguration<BranchEntity>
    {
        public void Configure(EntityTypeBuilder<BranchEntity> builder)
        {
            builder.ToTable("Branches");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.organizationId)
                .IsRequired();

            builder.Property(b => b.ArName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(b => b.EnName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(b => b.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasOne<OrganizationEntity>()
                .WithMany()
                .HasForeignKey(b => b.organizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
