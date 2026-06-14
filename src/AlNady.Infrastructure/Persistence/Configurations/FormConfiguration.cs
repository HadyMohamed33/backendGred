using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class FormConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.HasKey(f => f.FormId);
        builder.Property(f => f.FormId).ValueGeneratedOnAdd();

        builder.HasIndex(f => f.ProgramId).IsUnique();
        builder.Property(f => f.Title).IsRequired().HasMaxLength(300);
        builder.Property(f => f.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(f => f.Fields)
               .WithOne(ff => ff.Form)
               .HasForeignKey(ff => ff.FormId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Responses)
               .WithOne(fr => fr.Form)
               .HasForeignKey(fr => fr.FormId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Forms");
    }
}
