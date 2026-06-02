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
            .HasMaxLength(60);

        builder.Property(p => p.Breed)
            .HasMaxLength(100);

        builder.Property(p => p.WeightKg)
            .HasColumnType("numeric(5,2)");

        builder.Property(p => p.Born)
            .IsRequired();

        builder.Property(p => p.ImageUrl)
            .HasColumnType("text");

        builder.Property(p => p.Sex)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Sterilized)
            .IsRequired();

        builder.Property(p => p.MicrochipNumber)
            .HasMaxLength(40);

        builder.Property(p => p.Color)
            .HasMaxLength(60);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.Property(p => p.IsArchived)
            .IsRequired();

        builder.HasOne(p => p.Owner)
            .WithMany(o => o.Pets)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
