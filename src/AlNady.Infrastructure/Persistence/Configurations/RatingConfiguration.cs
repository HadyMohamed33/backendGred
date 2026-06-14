using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(r => r.RatingId);
        builder.Property(r => r.RatingId).ValueGeneratedOnAdd();

        builder.HasIndex(r => new { r.UserId, r.ProgramId }).IsUnique(); // One rating per user per program

        builder.Property(r => r.RatingValue)
               .IsRequired()
               .HasAnnotation("Range", new[] { 1, 5 });

        builder.HasCheckConstraint("CK_Rating_Value", "RatingValue >= 1 AND RatingValue <= 5");

        builder.Property(r => r.Comment).HasMaxLength(2000);
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(r => r.Aspects)
               .WithOne(a => a.Rating)
               .HasForeignKey(a => a.RatingId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Ratings");
    }
}
