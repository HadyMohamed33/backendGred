using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class EventLogConfiguration : IEntityTypeConfiguration<EventLog>
{
    public void Configure(EntityTypeBuilder<EventLog> builder)
    {
        builder.HasKey(e => e.LogId);
        builder.Property(e => e.LogId).ValueGeneratedOnAdd();

        builder.Property(e => e.Description).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.Property(e => e.UserAgent).HasMaxLength(512);
        builder.Property(e => e.AdditionalData).HasMaxLength(8000);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.EventType)
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.CreatedAt);

        builder.ToTable("EventLogs");
    }
}
