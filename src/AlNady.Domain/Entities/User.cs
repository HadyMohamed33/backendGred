using AlNady.Domain.Common;
using AlNady.Domain.Enums;

namespace AlNady.Domain.Entities;

public class User : AuditableEntity
{
    public int UserId { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public UserRole Role { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? NationalId { get; set; }

    // Navigation properties
    public Trainer? Trainer { get; set; }
    public Academy? Academy { get; set; }
    public ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();
    public ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
    public ICollection<Blacklist> Blacklists { get; set; } = new List<Blacklist>();
    public ICollection<FormResponse> FormResponses { get; set; } = new List<FormResponse>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
