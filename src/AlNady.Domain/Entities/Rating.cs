using AlNady.Domain.Common;

namespace AlNady.Domain.Entities;

public class Rating : AuditableEntity
{
    public int RatingId { get; set; }
    public int UserId { get; set; }
    public int ProgramId { get; set; }
    public int RatingValue { get; set; } // 1-5
    public string? Comment { get; set; }

    // Navigation
    public User User { get; set; } = default!;
    public TrainingProgram Program { get; set; } = default!;
    public ICollection<RatingAspect> Aspects { get; set; } = new List<RatingAspect>();
}
