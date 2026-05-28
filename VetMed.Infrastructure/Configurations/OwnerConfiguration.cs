using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetMed.Shared.Models;

namespace VetMed.Infrastructure.Configurations;

public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(o => o.Email)
            .IsUnique();

        builder.Property(o => o.PasswordHash)
            .IsRequired();

        builder.Property(o => o.Phone)
            .HasMaxLength(20);

        builder.Property(o => o.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}
