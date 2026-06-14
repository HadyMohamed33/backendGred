using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class VerificationCodeConfiguration : IEntityTypeConfiguration<VerificationCode>
{
    public void Configure(EntityTypeBuilder<VerificationCode> builder)
    {
        builder.HasKey(v => v.VerificationId);
        builder.Property(v => v.VerificationId).ValueGeneratedOnAdd();

        builder.Property(v => v.Code).IsRequired().HasMaxLength(10);
        builder.Property(v => v.Type).IsRequired().HasMaxLength(50);
        builder.Property(v => v.IsUsed).HasDefaultValue(false);
        builder.Property(v => v.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(v => new { v.UserId, v.Type, v.IsUsed });

        builder.ToTable("VerificationCodes");
    }
}
