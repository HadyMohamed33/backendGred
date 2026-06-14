using AlNady.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace AlNady.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
        _smtpHost = config["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.TryParse(config["EmailSettings:SmtpPort"], out var p) ? p : 587;
        _smtpUser = config["EmailSettings:SmtpUser"] ?? "";
        _smtpPass = config["EmailSettings:SmtpPass"] ?? "";
        _fromEmail = config["EmailSettings:FromEmail"] ?? "noreply@alnady.com";
        _fromName = config["EmailSettings:FromName"] ?? "AlNady Platform";
    }

    public async Task SendEmailVerificationAsync(string email, string name, string code, CancellationToken ct = default)
    {
        var subject = "Verify Your Email — AlNady";
        var body = $@"
            <h2>Welcome to AlNady, {name}!</h2>
            <p>Your email verification code is:</p>
            <h1 style='color:#2563eb;letter-spacing:8px;'>{code}</h1>
            <p>This code expires in 15 minutes.</p>
            <p>If you didn't register, please ignore this email.</p>";
        await SendGenericEmailAsync(email, subject, body, ct);
    }

    public async Task SendPasswordResetAsync(string email, string name, string code, CancellationToken ct = default)
    {
        var subject = "Password Reset Request — AlNady";
        var body = $@"
            <h2>Password Reset, {name}</h2>
            <p>Your password reset code is:</p>
            <h1 style='color:#dc2626;letter-spacing:8px;'>{code}</h1>
            <p>This code expires in 15 minutes. If you didn't request a reset, ignore this email.</p>";
        await SendGenericEmailAsync(email, subject, body, ct);
    }

    public async Task SendEnrollmentConfirmationAsync(string email, string name, string programTitle, CancellationToken ct = default)
    {
        var subject = "Enrollment Confirmed — AlNady";
        var body = $@"
            <h2>Enrollment Confirmed!</h2>
            <p>Hello {name}, you've successfully enrolled in <strong>{programTitle}</strong>.</p>
            <p>Check your dashboard for more details.</p>";
        await SendGenericEmailAsync(email, subject, body, ct);
    }

    public async Task SendPaymentReceiptAsync(string email, string name, string programTitle, decimal amount, string transactionId, CancellationToken ct = default)
    {
        var subject = "Payment Receipt — AlNady";
        var body = $@"
            <h2>Payment Receipt</h2>
            <p>Hello {name},</p>
            <p>Payment for <strong>{programTitle}</strong> was successful.</p>
            <table>
                <tr><td>Amount:</td><td><strong>{amount:C}</strong></td></tr>
                <tr><td>Transaction ID:</td><td>{transactionId}</td></tr>
            </table>";
        await SendGenericEmailAsync(email, subject, body, ct);
    }

    public async Task SendCancellationNoticeAsync(string email, string name, string programTitle, decimal refundAmount, CancellationToken ct = default)
    {
        var subject = "Enrollment Cancelled — AlNady";
        var body = $@"
            <h2>Enrollment Cancelled</h2>
            <p>Hello {name}, your enrollment in <strong>{programTitle}</strong> has been cancelled.</p>
            <p>Refund amount: <strong>{refundAmount:C}</strong> will be processed within 5-7 business days.</p>";
        await SendGenericEmailAsync(email, subject, body, ct);
    }

    public async Task SendWelcomeEmailAsync(string email, string name, CancellationToken ct = default)
    {
        var subject = "Welcome to AlNady!";
        var body = $@"
            <h2>Welcome to AlNady, {name}! 🎉</h2>
            <p>Your account has been created. Start exploring training programs today.</p>";
        await SendGenericEmailAsync(email, subject, body, ct);
    }

    public async Task SendGenericEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = WrapInTemplate(htmlBody) };

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_smtpUser, _smtpPass, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} with subject {Subject}", to, subject);
        }
    }

    private static string WrapInTemplate(string content) => $@"
        <!DOCTYPE html>
        <html>
        <head><meta charset='utf-8'><style>
            body {{ font-family: 'Segoe UI', Arial, sans-serif; background:#f8fafc; margin:0; padding:20px; }}
            .container {{ max-width:600px; margin:0 auto; background:white; border-radius:12px; padding:32px; box-shadow:0 2px 10px rgba(0,0,0,0.1); }}
            h1,h2 {{ color:#1e293b; }}
            p {{ color:#475569; line-height:1.6; }}
        </style></head>
        <body><div class='container'>{content}
            <hr style='border:none;border-top:1px solid #e2e8f0;margin:24px 0;'>
            <p style='font-size:12px;color:#94a3b8;'>© {DateTime.UtcNow.Year} AlNady Sports Training Platform</p>
        </div></body></html>";
}
