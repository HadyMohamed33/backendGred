using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class FieldValueConfiguration : IEntityTypeConfiguration<FieldValue>
{
    public void Configure(EntityTypeBuilder<FieldValue> builder)
    {
        builder.HasKey(fv => fv.ValueId);
        builder.Property(fv => fv.ValueId).ValueGeneratedOnAdd();

        builder.HasIndex(fv => new { fv.ResponseId, fv.FieldId }).IsUnique();

        builder.Property(fv => fv.Value).HasMaxLength(4000);

        builder.ToTable("FieldValues");
    }
}
