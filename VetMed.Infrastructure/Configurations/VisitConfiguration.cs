using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetMed.Shared.Models;

namespace VetMed.Infrastructure.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.ScheduledAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(v => v.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(v => v.Notes)
            .HasMaxLength(2000);

        builder.HasOne(v => v.Pet)
            .WithMany(p => p.Visits)
            .HasForeignKey(v => v.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Doctor)
            .WithMany(d => d.Visits)
            .HasForeignKey(v => v.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.ScheduledAt);
    }
}
