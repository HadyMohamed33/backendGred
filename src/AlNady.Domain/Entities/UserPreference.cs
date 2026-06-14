using AlNady.Domain.Common;

namespace AlNady.Domain.Entities;

public class UserPreference : BaseEntity
{
    public int PreferenceId { get; set; }
    public int UserId { get; set; }
    public string PreferenceKey { get; set; } = default!;
    public string? PreferenceValue { get; set; }

    // Navigation
    public User User { get; set; } = default!;
}
