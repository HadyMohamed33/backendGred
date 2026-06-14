using AlNady.Domain.Common;

namespace AlNady.Domain.Entities;

public class Certificate : BaseEntity
{
    public int CertificateId { get; set; }
    public int? TrainerId { get; set; }
    public int? AcademyId { get; set; }
    public string CertificateName { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public bool IsVerifiedByAdmin { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    // Navigation
    public Trainer? Trainer { get; set; }
    public Academy? Academy { get; set; }
}
