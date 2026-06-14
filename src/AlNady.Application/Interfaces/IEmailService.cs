namespace AlNady.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string name, string code, CancellationToken ct = default);
    Task SendPasswordResetAsync(string email, string name, string code, CancellationToken ct = default);
    Task SendEnrollmentConfirmationAsync(string email, string name, string programTitle, CancellationToken ct = default);
    Task SendPaymentReceiptAsync(string email, string name, string programTitle, decimal amount, string transactionId, CancellationToken ct = default);
    Task SendCancellationNoticeAsync(string email, string name, string programTitle, decimal refundAmount, CancellationToken ct = default);
    Task SendWelcomeEmailAsync(string email, string name, CancellationToken ct = default);
    Task SendGenericEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}
