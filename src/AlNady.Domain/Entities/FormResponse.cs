using AlNady.Domain.Common;
using AlNady.Domain.Enums;

namespace AlNady.Domain.Entities;

public class FormResponse : BaseEntity
{
    public int ResponseId { get; set; }
    public int FormId { get; set; }
    public int UserId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public FormResponseStatus Status { get; set; } = FormResponseStatus.Pending;

    // Navigation
    public Form Form { get; set; } = default!;
    public User User { get; set; } = default!;
    public ICollection<FieldValue> FieldValues { get; set; } = new List<FieldValue>();
    public Payment? Payment { get; set; }
    public Cancellation? Cancellation { get; set; }
}
