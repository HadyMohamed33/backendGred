using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class BlacklistConfiguration : IEntityTypeConfiguration<Blacklist>
{
    public void Configure(EntityTypeBuilder<Blacklist> builder)
    {
        builder.HasKey(b => b.BlacklistId);
        builder.Property(b => b.BlacklistId).ValueGeneratedOnAdd();

        builder.Property(b => b.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(b => b.IsActive).HasDefaultValue(true);
        builder.Property(b => b.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(b => new { b.UserId, b.IsActive });

        builder.ToTable("Blacklists");
    }
}
