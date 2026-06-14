using AlNady.Domain.Common;
using AlNady.Domain.Enums;

namespace AlNady.Domain.Entities;

public class TrainingProgram : AuditableEntity
{
    public int ProgramId { get; set; }
    public int? TrainerId { get; set; }
    public int? AcademyId { get; set; }
    public DateTime ProgramDate { get; set; }
    public TimeSpan ProgramTime { get; set; }
    public decimal Price { get; set; }
    public ProgramStatus Status { get; set; } = ProgramStatus.Draft;
    public int Capacity { get; set; }
    public int AvailableSlots { get; set; }
    public string? TrainingLocation { get; set; }
    public string? Description { get; set; }
    public string? Title { get; set; }
    public string? SportType { get; set; }
    public string? AgeCategory { get; set; }
    public string? GenderPreference { get; set; }

    // Navigation
    public Trainer? Trainer { get; set; }
    public Academy? Academy { get; set; }
    public Form? Form { get; set; }
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
