using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class CancellationConfiguration : IEntityTypeConfiguration<Cancellation>
{
    public void Configure(EntityTypeBuilder<Cancellation> builder)
    {
        builder.HasKey(c => c.CancellationId);
        builder.Property(c => c.CancellationId).ValueGeneratedOnAdd();

        builder.HasIndex(c => c.ResponseId).IsUnique();

        builder.Property(c => c.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.RefundAmount).HasPrecision(10, 2).HasDefaultValue(0);
        builder.Property(c => c.IsRefundProcessed).HasDefaultValue(false);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.CancelledBy)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.ToTable("Cancellations");
    }
}
