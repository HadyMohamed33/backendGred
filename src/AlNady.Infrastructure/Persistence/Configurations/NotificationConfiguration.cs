using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.NotificationId);
        builder.Property(n => n.NotificationId).ValueGeneratedOnAdd();

        builder.Property(n => n.Message).IsRequired().HasMaxLength(2000);
        builder.Property(n => n.Title).HasMaxLength(300);
        builder.Property(n => n.ActionUrl).HasMaxLength(1024);
        builder.Property(n => n.IsRead).HasDefaultValue(false);
        builder.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.Property(n => n.Type)
               .HasConversion<string>()
               .HasMaxLength(30);

        builder.HasIndex(n => new { n.UserId, n.IsRead });

        builder.ToTable("Notifications");
    }
}
