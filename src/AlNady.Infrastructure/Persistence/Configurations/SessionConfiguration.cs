using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.SessionId);
        builder.Property(s => s.SessionId).ValueGeneratedOnAdd();

        builder.Property(s => s.RefreshToken).IsRequired().HasMaxLength(512);
        builder.HasIndex(s => s.RefreshToken).IsUnique();

        builder.Property(s => s.IpAddress).HasMaxLength(45);
        builder.Property(s => s.UserAgent).HasMaxLength(512);
        builder.Property(s => s.IsActive).HasDefaultValue(true);
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(s => new { s.UserId, s.IsActive });

        builder.ToTable("Sessions");
    }
}
