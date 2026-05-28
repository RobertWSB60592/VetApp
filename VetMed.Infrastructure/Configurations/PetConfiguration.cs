using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetMed.Shared.Enums;
using VetMed.Shared.Models;

namespace VetMed.Infrastructure.Configurations;

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Species)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.Breed)
            .HasMaxLength(100);

        builder.Property(p => p.WeightKg)
            .HasColumnType("numeric(5,2)");

        builder.Property(p => p.Born)
            .IsRequired();

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        builder.HasOne(p => p.Owner)
            .WithMany(o => o.Pets)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
