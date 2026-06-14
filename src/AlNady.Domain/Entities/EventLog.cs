using AlNady.Domain.Common;
using AlNady.Domain.Enums;

namespace AlNady.Domain.Entities;

public class EventLog : BaseEntity
{
    public int LogId { get; set; }
    public int? UserId { get; set; }
    public EventType EventType { get; set; }
    public string Description { get; set; } = default!;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? AdditionalData { get; set; } // JSON
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
}
