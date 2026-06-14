using AlNady.Domain.Common;
using AlNady.Domain.Enums;

namespace AlNady.Domain.Entities;

public class Payment : AuditableEntity
{
    public int PaymentId { get; set; }
    public int ResponseId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public string? ExternalPaymentId { get; set; } // Paymob/Stripe order id
    public string? ProviderName { get; set; } // "Paymob" | "Stripe"
    public string? ReceiptUrl { get; set; }
    public DateTime? PaidAt { get; set; }

    // Navigation
    public FormResponse Response { get; set; } = default!;
}
