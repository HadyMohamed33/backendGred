using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class FormResponseConfiguration : IEntityTypeConfiguration<FormResponse>
{
    public void Configure(EntityTypeBuilder<FormResponse> builder)
    {
        builder.HasKey(r => r.ResponseId);
        builder.Property(r => r.ResponseId).ValueGeneratedOnAdd();

        builder.HasIndex(r => new { r.FormId, r.UserId }).IsUnique(); // One response per user per form

        builder.Property(r => r.SubmittedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(r => r.Status)
               .HasConversion<string>()
               .HasMaxLength(30)
               .HasDefaultValue(FormResponseStatus.Pending);

        builder.HasMany(r => r.FieldValues)
               .WithOne(fv => fv.Response)
               .HasForeignKey(fv => fv.ResponseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Payment)
               .WithOne(p => p.Response)
               .HasForeignKey<Payment>(p => p.ResponseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Cancellation)
               .WithOne(c => c.Response)
               .HasForeignKey<Cancellation>(c => c.ResponseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("FormResponses");
    }
}
