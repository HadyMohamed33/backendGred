using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.PaymentId);
        builder.Property(p => p.PaymentId).ValueGeneratedOnAdd();

        builder.HasIndex(p => p.ResponseId).IsUnique();
        builder.HasIndex(p => p.TransactionId);

        builder.Property(p => p.Amount).HasPrecision(10, 2);
        builder.Property(p => p.TransactionId).HasMaxLength(256);
        builder.Property(p => p.ExternalPaymentId).HasMaxLength(256);
        builder.Property(p => p.ProviderName).HasMaxLength(50);
        builder.Property(p => p.ReceiptUrl).HasMaxLength(1024);

        builder.Property(p => p.PaymentMethod)
               .HasConversion<string>()
               .HasMaxLength(30);

        builder.Property(p => p.Status)
               .HasConversion<string>()
               .HasMaxLength(30)
               .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.ToTable("Payments");
    }
}
