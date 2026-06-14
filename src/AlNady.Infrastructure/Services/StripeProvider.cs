using AlNady.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace AlNady.Infrastructure.Services;

/// <summary>
/// Stripe payment provider implementation.
/// </summary>
public class StripeProvider : IPaymentService
{
    private readonly IConfiguration _config;
    private readonly ILogger<StripeProvider> _logger;
    private readonly string _webhookSecret;

    public string ProviderName => "Stripe";

    public StripeProvider(IConfiguration config, ILogger<StripeProvider> logger)
    {
        _config = config;
        _logger = logger;
        StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"] ?? "";
        _webhookSecret = config["StripeSettings:WebhookSecret"] ?? "";
    }

    public async Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentInitiationRequest request, CancellationToken ct = default)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(request.Amount * 100),
                            Currency = request.Currency.ToLower(),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = request.OrderDescription
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                CustomerEmail = request.CustomerEmail,
                SuccessUrl = _config["StripeSettings:SuccessUrl"] + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = _config["StripeSettings:CancelUrl"],
                Metadata = new Dictionary<string, string>
                {
                    { "response_id", request.ResponseId.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options, cancellationToken: ct);

            return new PaymentInitiationResult(true, session.Url, session.Id, null);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment initiation failed");
            return new PaymentInitiationResult(false, null, null, ex.Message);
        }
    }

    public Task<WebhookVerificationResult> VerifyWebhookAsync(string payload, string signature, CancellationToken ct = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _webhookSecret);

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                return Task.FromResult(new WebhookVerificationResult(
                    true,
                    session?.PaymentIntentId,
                    session?.Id,
                    true,
                    session != null ? (decimal)session.AmountTotal!.Value / 100 : null
                ));
            }

            return Task.FromResult(new WebhookVerificationResult(true, null, null, false, null));
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook verification failed");
            return Task.FromResult(new WebhookVerificationResult(false, null, null, false, null));
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(RefundRequest request, CancellationToken ct = default)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = request.TransactionId,
                Amount = (long)(request.Amount * 100),
                Reason = RefundReasons.RequestedByCustomer
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options, cancellationToken: ct);

            return new RefundResult(refund.Status == "succeeded", refund.Id, null);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe refund failed");
            return new RefundResult(false, null, ex.Message);
        }
    }
}
