using AlNady.Domain.Common;

namespace AlNady.Domain.Entities;

public class FieldValue : BaseEntity
{
    public int ValueId { get; set; }
    public int ResponseId { get; set; }
    public int FieldId { get; set; }
    public string? Value { get; set; }

    // Navigation
    public FormResponse Response { get; set; } = default!;
    public FormField Field { get; set; } = default!;
}
