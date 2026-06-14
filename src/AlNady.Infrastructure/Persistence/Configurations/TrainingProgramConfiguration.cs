using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class TrainingProgramConfiguration : IEntityTypeConfiguration<TrainingProgram>
{
    public void Configure(EntityTypeBuilder<TrainingProgram> builder)
    {
        builder.HasKey(p => p.ProgramId);
        builder.Property(p => p.ProgramId).ValueGeneratedOnAdd();

        builder.Property(p => p.Title).HasMaxLength(300);
        builder.Property(p => p.Description).HasMaxLength(5000);
        builder.Property(p => p.TrainingLocation).HasMaxLength(500);
        builder.Property(p => p.SportType).HasMaxLength(100);
        builder.Property(p => p.AgeCategory).HasMaxLength(100);
        builder.Property(p => p.GenderPreference).HasMaxLength(20);

        builder.Property(p => p.Price)
               .HasPrecision(10, 2)
               .HasDefaultValue(0);

        builder.Property(p => p.Status)
               .HasConversion<string>()
               .HasMaxLength(30)
               .HasDefaultValue(ProgramStatus.Draft);

        builder.Property(p => p.Capacity).HasDefaultValue(0);
        builder.Property(p => p.AvailableSlots).HasDefaultValue(0);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Either TrainerId or AcademyId
        builder.HasCheckConstraint("CK_TrainingProgram_Owner",
            "TrainerId IS NOT NULL OR AcademyId IS NOT NULL");

        builder.HasOne(p => p.Form)
               .WithOne(f => f.Program)
               .HasForeignKey<Form>(f => f.ProgramId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Ratings)
               .WithOne(r => r.Program)
               .HasForeignKey(r => r.ProgramId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("TrainingPrograms");
    }
}
