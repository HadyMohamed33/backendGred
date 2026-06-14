using AlNady.Application.Interfaces;
using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using AlNady.Shared.Common;
using AlNady.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlNady.Application.Features.Payments.DTOs
{
    public record PaymentDto(int PaymentId, int ResponseId, decimal Amount, string PaymentMethod, string Status, string? TransactionId, string? ReceiptUrl, DateTime CreatedAt);
    public record InitiatePaymentRequest(int ResponseId, PaymentMethod Method);
    public record CancellationDto(int CancellationId, int ResponseId, string Reason, string CancelledBy, decimal RefundAmount, bool IsRefundProcessed, DateTime CreatedAt);
    public record CancelEnrollmentRequest(string Reason);
    public record InitiatePaymentResultDto(string? PaymentUrl, string? ExternalPaymentId);
}

namespace AlNady.Application.Features.Payments.Commands
{
    public record InitiatePaymentCommand(int ResponseId, int UserId, PaymentMethod Method) : IRequest<Result<DTOs.InitiatePaymentResultDto>>;
    public record ProcessWebhookCommand(string Provider, string Payload, string Signature) : IRequest<Result>;
    public record CancelEnrollmentCommand(int ResponseId, int UserId, string Reason, CancelledBy CancelledBy) : IRequest<Result<DTOs.CancellationDto>>;
}

namespace AlNady.Application.Features.Payments.Queries
{
    public record GetPaymentStatusQuery(int ResponseId, int UserId) : IRequest<Result<DTOs.PaymentDto>>;
}

namespace AlNady.Application.Features.Payments.Handlers
{
    using DTOs;

    public class InitiatePaymentCommandHandler : IRequestHandler<Commands.InitiatePaymentCommand, Result<InitiatePaymentResultDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEnumerable<IPaymentService> _paymentServices;
        private readonly IAuditService _audit;

        public InitiatePaymentCommandHandler(IApplicationDbContext context, IEnumerable<IPaymentService> paymentServices, IAuditService audit)
        { _context = context; _paymentServices = paymentServices; _audit = audit; }

        public async Task<Result<InitiatePaymentResultDto>> Handle(Commands.InitiatePaymentCommand request, CancellationToken ct)
        {
            var response = await _context.FormResponses
                .Include(r => r.Form).ThenInclude(f => f.Program)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ResponseId == request.ResponseId, ct);

            if (response == null) return Result<InitiatePaymentResultDto>.NotFound("Enrollment not found.");
            if (response.UserId != request.UserId) return Result<InitiatePaymentResultDto>.Forbidden();
            if (response.Status != FormResponseStatus.PendingPayment && response.Status != FormResponseStatus.Approved)
                return Result<InitiatePaymentResultDto>.Failure("Enrollment is not in a payable state.");
            if (await _context.Payments.AnyAsync(p => p.ResponseId == request.ResponseId &&
                (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Completed), ct))
                return Result<InitiatePaymentResultDto>.Conflict("Payment already exists for this enrollment.");

            var program = response.Form.Program;
            var user = response.User;
            var providerName = request.Method == PaymentMethod.Stripe ? "Stripe" : "Paymob";
            var provider = _paymentServices.FirstOrDefault(p => p.ProviderName == providerName) ?? _paymentServices.First();

            var paymentRequest = new PaymentInitiationRequest(request.ResponseId, program.Price, "EGP", user.Email, user.FullName, user.Phone ?? "", $"Enrollment for {program.Title}");
            var result = await provider.InitiatePaymentAsync(paymentRequest, ct);
            if (!result.IsSuccess) return Result<InitiatePaymentResultDto>.Failure(result.ErrorMessage ?? "Payment initiation failed.");

            var payment = new Payment { ResponseId = request.ResponseId, Amount = program.Price, PaymentMethod = request.Method, Status = PaymentStatus.Pending, ExternalPaymentId = result.ExternalPaymentId, ProviderName = provider.ProviderName, CreatedAt = DateTime.UtcNow };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(ct);
            await _audit.LogAsync(request.UserId, EventType.PaymentInitiated, $"Payment initiated for enrollment {request.ResponseId}");
            return Result<InitiatePaymentResultDto>.Success(new InitiatePaymentResultDto(result.PaymentUrl, result.ExternalPaymentId));
        }
    }

    public class ProcessWebhookCommandHandler : IRequestHandler<Commands.ProcessWebhookCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEnumerable<IPaymentService> _paymentServices;
        private readonly INotificationService _notifications;
        private readonly IAuditService _audit;

        public ProcessWebhookCommandHandler(IApplicationDbContext context, IEnumerable<IPaymentService> paymentServices, INotificationService notifications, IAuditService audit)
        { _context = context; _paymentServices = paymentServices; _notifications = notifications; _audit = audit; }

        public async Task<Result> Handle(Commands.ProcessWebhookCommand request, CancellationToken ct)
        {
            var provider = _paymentServices.FirstOrDefault(p => p.ProviderName == request.Provider);
            if (provider == null) return Result.Failure("Unknown provider.");
            var webhookResult = await provider.VerifyWebhookAsync(request.Payload, request.Signature, ct);
            if (!webhookResult.IsValid) return Result.Failure("Invalid webhook signature.");
            if (webhookResult.ExternalPaymentId == null) return Result.Success();
            var payment = await _context.Payments.Include(p => p.Response).ThenInclude(r => r.Form).ThenInclude(f => f.Program)
                .Include(p => p.Response).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.ExternalPaymentId == webhookResult.ExternalPaymentId, ct);
            if (payment == null) return Result.Success();
            if (webhookResult.IsSuccessfulPayment && payment.Status == PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Completed; payment.TransactionId = webhookResult.TransactionId; payment.PaidAt = DateTime.UtcNow;
                payment.Response.Status = FormResponseStatus.Enrolled;
                await _context.SaveChangesAsync(ct);
                _ = Task.Run(async () => await _notifications.SendAsync(payment.Response.UserId, "Payment Successful", $"Payment confirmed for '{payment.Response.Form.Program.Title}'.", NotificationType.Payment));
                await _audit.LogAsync(payment.Response.UserId, EventType.PaymentCompleted, $"Payment completed: {payment.TransactionId}");
            }
            else if (!webhookResult.IsSuccessfulPayment)
            {
                payment.Status = PaymentStatus.Failed;
                await _context.SaveChangesAsync(ct);
                await _audit.LogAsync(payment.Response.UserId, EventType.PaymentFailed, $"Payment failed for enrollment {payment.ResponseId}");
            }
            return Result.Success();
        }
    }

    public class GetPaymentStatusQueryHandler : IRequestHandler<Queries.GetPaymentStatusQuery, Result<PaymentDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetPaymentStatusQueryHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result<PaymentDto>> Handle(Queries.GetPaymentStatusQuery request, CancellationToken ct)
        {
            var payment = await _context.Payments.AsNoTracking().Include(p => p.Response)
                .FirstOrDefaultAsync(p => p.ResponseId == request.ResponseId, ct);
            if (payment == null) return Result<PaymentDto>.NotFound("No payment found for this enrollment.");
            if (payment.Response.UserId != request.UserId) return Result<PaymentDto>.Forbidden();
            return Result<PaymentDto>.Success(new PaymentDto(payment.PaymentId, payment.ResponseId, payment.Amount,
                payment.PaymentMethod.ToString(), payment.Status.ToString(), payment.TransactionId, payment.ReceiptUrl, payment.CreatedAt));
        }
    }

    public class CancelEnrollmentCommandHandler : IRequestHandler<Commands.CancelEnrollmentCommand, Result<CancellationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEnumerable<IPaymentService> _paymentServices;
        private readonly INotificationService _notifications;
        private readonly IEmailService _email;
        private readonly IAuditService _audit;

        public CancelEnrollmentCommandHandler(IApplicationDbContext context, IEnumerable<IPaymentService> paymentServices, INotificationService notifications, IEmailService email, IAuditService audit)
        { _context = context; _paymentServices = paymentServices; _notifications = notifications; _email = email; _audit = audit; }

        public async Task<Result<CancellationDto>> Handle(Commands.CancelEnrollmentCommand request, CancellationToken ct)
        {
            var response = await _context.FormResponses
                .Include(r => r.Form).ThenInclude(f => f.Program).Include(r => r.User)
                .Include(r => r.Payment).Include(r => r.Cancellation)
                .FirstOrDefaultAsync(r => r.ResponseId == request.ResponseId, ct);
            if (response == null) return Result<CancellationDto>.NotFound("Enrollment not found.");
            if (response.Cancellation != null) return Result<CancellationDto>.Conflict("Enrollment already cancelled.");
            if (response.Status == FormResponseStatus.Cancelled) return Result<CancellationDto>.Conflict("Already cancelled.");

            var program = response.Form.Program;
            var daysUntilProgram = (program.ProgramDate - DateTime.UtcNow).TotalDays;
            decimal refundAmount = 0;
            if (response.Payment?.Status == PaymentStatus.Completed)
            {
                if (daysUntilProgram >= AppConstants.Refund.FullRefundDaysBeforeProgram) refundAmount = response.Payment.Amount;
                else if (daysUntilProgram >= AppConstants.Refund.PartialRefundDaysBeforeProgram) refundAmount = response.Payment.Amount * AppConstants.Refund.PartialRefundPercentage;
            }

            response.Status = FormResponseStatus.Cancelled;
            program.AvailableSlots++;
            if (program.Status == ProgramStatus.Full) program.Status = ProgramStatus.Published;

            var cancellation = new Cancellation { ResponseId = request.ResponseId, Reason = request.Reason, CancelledBy = request.CancelledBy, CancelledByUserId = request.UserId, RefundAmount = refundAmount, IsRefundProcessed = false, CreatedAt = DateTime.UtcNow };
            _context.Cancellations.Add(cancellation);
            await _context.SaveChangesAsync(ct);

            if (refundAmount > 0 && response.Payment?.TransactionId != null)
            {
                var provider = _paymentServices.FirstOrDefault(p => p.ProviderName == response.Payment.ProviderName) ?? _paymentServices.First();
                var refundResult = await provider.ProcessRefundAsync(new RefundRequest(response.Payment.TransactionId, refundAmount, request.Reason), ct);
                if (refundResult.IsSuccess) { cancellation.IsRefundProcessed = true; response.Payment.Status = PaymentStatus.Refunded; await _context.SaveChangesAsync(ct); await _audit.LogAsync(request.UserId, EventType.RefundProcessed, $"Refund {refundAmount} processed"); }
            }

            var user = response.User;
            _ = Task.Run(async () =>
            {
                await _notifications.SendAsync(user.UserId, "Enrollment Cancelled", $"Your enrollment in '{program.Title}' was cancelled.", NotificationType.Cancellation);
                await _email.SendCancellationNoticeAsync(user.Email, user.FullName, program.Title ?? "Program", refundAmount);
            });

            await _audit.LogAsync(request.UserId, EventType.EnrollmentCancelled, $"Enrollment {request.ResponseId} cancelled");
            return Result<CancellationDto>.Success(new CancellationDto(cancellation.CancellationId, cancellation.ResponseId, cancellation.Reason, cancellation.CancelledBy.ToString(), cancellation.RefundAmount, cancellation.IsRefundProcessed, cancellation.CreatedAt));
        }
    }
}
