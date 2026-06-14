using AlNady.Domain.Common;
using AlNady.Domain.Enums;

namespace AlNady.Domain.Entities;

public class FormField : BaseEntity
{
    public int FieldId { get; set; }
    public int FormId { get; set; }
    public string Label { get; set; } = default!;
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? Options { get; set; } // JSON array for Select type
    public string? Placeholder { get; set; }

    // Navigation
    public Form Form { get; set; } = default!;
    public ICollection<FieldValue> FieldValues { get; set; } = new List<FieldValue>();
}
