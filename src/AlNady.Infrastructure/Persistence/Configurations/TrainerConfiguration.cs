using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        builder.HasKey(t => t.TrainerId);
        builder.Property(t => t.TrainerId).ValueGeneratedOnAdd();

        builder.HasIndex(t => t.UserId).IsUnique();

        builder.Property(t => t.About).HasMaxLength(2000);
        builder.Property(t => t.SpecializationSports).HasMaxLength(500);
        builder.Property(t => t.AgeCategory).HasMaxLength(100);
        builder.Property(t => t.GenderPreference).HasMaxLength(20);
        builder.Property(t => t.AverageRating).HasPrecision(3, 2).HasDefaultValue(0);
        builder.Property(t => t.IsVerifiedByAdmin).HasDefaultValue(false);

        builder.HasMany(t => t.Certificates)
               .WithOne(c => c.Trainer)
               .HasForeignKey(c => c.TrainerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.TrainingPrograms)
               .WithOne(p => p.Trainer)
               .HasForeignKey(p => p.TrainerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Trainers");
    }
}
