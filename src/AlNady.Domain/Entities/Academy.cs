using AlNady.Domain.Common;

namespace AlNady.Domain.Entities;

public class Academy : AuditableEntity
{
    public int AcademyId { get; set; }
    public int UserId { get; set; }
    public string? SpecializationSports { get; set; }
    public bool IsVerified { get; set; }
    public decimal AverageRating { get; set; }
    public string? Location { get; set; }
    public string? AgeCategory { get; set; }
    public string? GenderPreference { get; set; }

    // Navigation
    public User User { get; set; } = default!;
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    public ICollection<TrainingProgram> TrainingPrograms { get; set; } = new List<TrainingProgram>();
}
