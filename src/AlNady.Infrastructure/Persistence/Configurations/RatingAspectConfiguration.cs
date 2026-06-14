using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class RatingAspectConfiguration : IEntityTypeConfiguration<RatingAspect>
{
    public void Configure(EntityTypeBuilder<RatingAspect> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.HasIndex(a => new { a.RatingId, a.AspectName }).IsUnique();

        builder.Property(a => a.AspectName).IsRequired().HasMaxLength(100);
        builder.HasCheckConstraint("CK_RatingAspect_Value", "RatingValue >= 1 AND RatingValue <= 5");

        builder.ToTable("RatingAspects");
    }
}
