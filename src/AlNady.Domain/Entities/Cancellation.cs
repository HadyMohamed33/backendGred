using AlNady.Domain.Common;
using AlNady.Domain.Enums;

namespace AlNady.Domain.Entities;

public class Cancellation : BaseEntity
{
    public int CancellationId { get; set; }
    public int ResponseId { get; set; }
    public string Reason { get; set; } = default!;
    public CancelledBy CancelledBy { get; set; }
    public int CancelledByUserId { get; set; }
    public decimal RefundAmount { get; set; }
    public bool IsRefundProcessed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public FormResponse Response { get; set; } = default!;
}
