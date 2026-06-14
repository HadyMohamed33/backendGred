using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.HasKey(f => f.FieldId);
        builder.Property(f => f.FieldId).ValueGeneratedOnAdd();

        builder.Property(f => f.Label).IsRequired().HasMaxLength(300);
        builder.Property(f => f.Placeholder).HasMaxLength(300);
        builder.Property(f => f.Options).HasMaxLength(4000); // JSON array

        builder.Property(f => f.FieldType)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(f => f.IsRequired).HasDefaultValue(false);
        builder.Property(f => f.DisplayOrder).HasDefaultValue(0);

        builder.HasMany(f => f.FieldValues)
               .WithOne(fv => fv.Field)
               .HasForeignKey(fv => fv.FieldId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("FormFields");
    }
}
