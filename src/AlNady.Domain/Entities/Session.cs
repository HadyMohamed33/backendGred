using AlNady.Domain.Common;

namespace AlNady.Domain.Entities;

public class Session : BaseEntity
{
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public string RefreshToken { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = default!;
}
