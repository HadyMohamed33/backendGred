using AlNady.Domain.Common;
using AlNady.Domain.Enums;

namespace AlNady.Domain.Entities;

public class Notification : BaseEntity
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = default!;
    public string? Title { get; set; }
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    // Navigation
    public User User { get; set; } = default!;
}
