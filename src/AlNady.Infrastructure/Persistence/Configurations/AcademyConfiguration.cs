using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class AcademyConfiguration : IEntityTypeConfiguration<Academy>
{
    public void Configure(EntityTypeBuilder<Academy> builder)
    {
        builder.HasKey(a => a.AcademyId);
        builder.Property(a => a.AcademyId).ValueGeneratedOnAdd();

        builder.HasIndex(a => a.UserId).IsUnique();

        builder.Property(a => a.SpecializationSports).HasMaxLength(500);
        builder.Property(a => a.Location).HasMaxLength(500);
        builder.Property(a => a.AgeCategory).HasMaxLength(100);
        builder.Property(a => a.GenderPreference).HasMaxLength(20);
        builder.Property(a => a.AverageRating).HasPrecision(3, 2).HasDefaultValue(0);
        builder.Property(a => a.IsVerified).HasDefaultValue(false);

        builder.HasMany(a => a.Certificates)
               .WithOne(c => c.Academy)
               .HasForeignKey(c => c.AcademyId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.TrainingPrograms)
               .WithOne(p => p.Academy)
               .HasForeignKey(p => p.AcademyId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Academies");
    }
}
