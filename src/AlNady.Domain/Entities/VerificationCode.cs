using AlNady.Domain.Common;

namespace AlNady.Domain.Entities;

public class VerificationCode : BaseEntity
{
    public int VerificationId { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = default!;
    public string Type { get; set; } = default!; // EmailVerification, PasswordReset
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = default!;
}
