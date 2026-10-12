using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBRS.Domain.Common;

namespace SBRS.Infrastructure.Persistence.Configurations;

public abstract class LookupConfiguration<T> : IEntityTypeConfiguration<T> where T : LookupEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
       builder.HasKey(e => e.Id);
        builder.Property(e => e.ArName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.EnName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(16);
        builder.Property(e => e.CreatedAt).IsRequired();

        ConfigureEntity(builder);
    }
    protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
}