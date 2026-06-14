using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlNady.Infrastructure.Persistence.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(c => c.CertificateId);
        builder.Property(c => c.CertificateId).ValueGeneratedOnAdd();

        builder.Property(c => c.CertificateName).IsRequired().HasMaxLength(300);
        builder.Property(c => c.FilePath).IsRequired().HasMaxLength(1024);
        builder.Property(c => c.IsVerifiedByAdmin).HasDefaultValue(false);
        builder.Property(c => c.DateAdded).HasDefaultValueSql("GETUTCDATE()");

        // Either TrainerId or AcademyId must be set (not both null, enforced at app layer)
        builder.HasCheckConstraint("CK_Certificate_Owner",
            "TrainerId IS NOT NULL OR AcademyId IS NOT NULL");

        builder.ToTable("Certificates");
    }
}
