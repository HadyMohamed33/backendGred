using AlNady.Domain.Enums;

namespace AlNady.Application.Interfaces;

public record PaymentInitiationRequest(
    int ResponseId,
    decimal Amount,
    string Currency,
    string CustomerEmail,
    string CustomerName,
    string CustomerPhone,
    string OrderDescription
);

public record PaymentInitiationResult(
    bool IsSuccess,
    string? PaymentUrl,
    string? ExternalPaymentId,
    string? ErrorMessage
);

public record WebhookVerificationResult(
    bool IsValid,
    string? TransactionId,
    string? ExternalPaymentId,
    bool IsSuccessfulPayment,
    decimal? Amount
);

public record RefundRequest(
    string TransactionId,
    decimal Amount,
    string Reason
);

public record RefundResult(
    bool IsSuccess,
    string? RefundId,
    string? ErrorMessage
);

public interface IPaymentService
{
    Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentInitiationRequest request, CancellationToken ct = default);
    Task<WebhookVerificationResult> VerifyWebhookAsync(string payload, string signature, CancellationToken ct = default);
    Task<RefundResult> ProcessRefundAsync(RefundRequest request, CancellationToken ct = default);
    string ProviderName { get; }
}
