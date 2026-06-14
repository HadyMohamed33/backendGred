using AlNady.Domain.Common;

namespace AlNady.Domain.Entities;

public class Blacklist : BaseEntity
{
    public int BlacklistId { get; set; }
    public int UserId { get; set; }
    public string Reason { get; set; } = default!;
    public int BlacklistedByAdminId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation
    public User User { get; set; } = default!;
}
