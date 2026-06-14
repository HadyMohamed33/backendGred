using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.HasKey(p => p.PreferenceId);
        builder.Property(p => p.PreferenceId).ValueGeneratedOnAdd();

        builder.HasIndex(p => new { p.UserId, p.PreferenceKey }).IsUnique();

        builder.Property(p => p.PreferenceKey).IsRequired().HasMaxLength(100);
        builder.Property(p => p.PreferenceValue).HasMaxLength(2000);

        builder.ToTable("UserPreferences");
    }
}
