using AlNady.Domain.Common;

namespace AlNady.Domain.Entities;

public class RatingAspect : BaseEntity
{
    public int RatingId { get; set; }
    public string AspectName { get; set; } = default!; // Coaching, Facilities, Value, Communication
    public int RatingValue { get; set; } // 1-5

    // Navigation
    public Rating Rating { get; set; } = default!;
}
