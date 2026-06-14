using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId).ValueGeneratedOnAdd();

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.ProfileImage).HasMaxLength(512);
        builder.Property(u => u.NationalId).HasMaxLength(50);
        builder.HasIndex(u => u.NationalId);

        builder.Property(u => u.Role)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(u => u.IsVerified).HasDefaultValue(false);

        // Relationships
        builder.HasOne(u => u.Trainer)
               .WithOne(t => t.User)
               .HasForeignKey<Trainer>(t => t.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Academy)
               .WithOne(a => a.User)
               .HasForeignKey<Academy>(a => a.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.VerificationCodes)
               .WithOne(v => v.User)
               .HasForeignKey(v => v.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Sessions)
               .WithOne(s => s.User)
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Notifications)
               .WithOne(n => n.User)
               .HasForeignKey(n => n.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.EventLogs)
               .WithOne(e => e.User)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.Preferences)
               .WithOne(p => p.User)
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Blacklists)
               .WithOne(b => b.User)
               .HasForeignKey(b => b.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.FormResponses)
               .WithOne(fr => fr.User)
               .HasForeignKey(fr => fr.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Ratings)
               .WithOne(r => r.User)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Users");
    }
}
