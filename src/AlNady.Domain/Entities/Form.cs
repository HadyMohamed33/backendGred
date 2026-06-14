using AlNady.Domain.Common;

namespace AlNady.Domain.Entities;

public class Form : BaseEntity
{
    public int FormId { get; set; }
    public int ProgramId { get; set; }
    public string Title { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public TrainingProgram Program { get; set; } = default!;
    public ICollection<FormField> Fields { get; set; } = new List<FormField>();
    public ICollection<FormResponse> Responses { get; set; } = new List<FormResponse>();
}
