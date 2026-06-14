using AlNady.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AlNady.Infrastructure.Services;

/// <summary>
/// Paymob payment provider implementation.
/// </summary>
public class PaymobProvider : IPaymentService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymobProvider> _logger;
    private readonly string _apiKey;
    private readonly string _integrationId;
    private readonly string _hmacSecret;

    public string ProviderName => "Paymob";

    public PaymobProvider(IHttpClientFactory httpFactory, IConfiguration config, ILogger<PaymobProvider> logger)
    {
        _http = httpFactory.CreateClient("Paymob");
        _config = config;
        _logger = logger;
        _apiKey = config["PaymobSettings:ApiKey"] ?? "";
        _integrationId = config["PaymobSettings:IntegrationId"] ?? "";
        _hmacSecret = config["PaymobSettings:HmacSecret"] ?? "";
    }

    public async Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentInitiationRequest request, CancellationToken ct = default)
    {
        try
        {
            // Step 1: Auth token
            var authResponse = await _http.PostAsync("auth/tokens",
                new StringContent(JsonSerializer.Serialize(new { api_key = _apiKey }), Encoding.UTF8, "application/json"), ct);

            var authContent = await authResponse.Content.ReadAsStringAsync(ct);
            var authDoc = JsonDocument.Parse(authContent);
            var token = authDoc.RootElement.GetProperty("token").GetString();

            // Step 2: Order registration
            var orderData = new
            {
                auth_token = token,
                delivery_needed = false,
                amount_cents = (int)(request.Amount * 100),
                currency = request.Currency,
                items = Array.Empty<object>()
            };

            var orderResponse = await _http.PostAsync("ecommerce/orders",
                new StringContent(JsonSerializer.Serialize(orderData), Encoding.UTF8, "application/json"), ct);
            var orderContent = await orderResponse.Content.ReadAsStringAsync(ct);
            var orderDoc = JsonDocument.Parse(orderContent);
            var orderId = orderDoc.RootElement.GetProperty("id").GetInt64();

            // Step 3: Payment key
            var paymentKeyData = new
            {
                auth_token = token,
                amount_cents = (int)(request.Amount * 100),
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    email = request.CustomerEmail,
                    first_name = request.CustomerName.Split(' ').FirstOrDefault() ?? request.CustomerName,
                    last_name = request.CustomerName.Split(' ').LastOrDefault() ?? request.CustomerName,
                    phone_number = request.CustomerPhone,
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    city = "NA",
                    country = "EG",
                    shipping_method = "NA",
                    postal_code = "NA",
                    state = "NA"
                },
                currency = request.Currency,
                integration_id = _integrationId
            };

            var keyResponse = await _http.PostAsync("acceptance/payment_keys",
                new StringContent(JsonSerializer.Serialize(paymentKeyData), Encoding.UTF8, "application/json"), ct);
            var keyContent = await keyResponse.Content.ReadAsStringAsync(ct);
            var keyDoc = JsonDocument.Parse(keyContent);
            var paymentKey = keyDoc.RootElement.GetProperty("token").GetString();

            var paymentUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_integrationId}?payment_token={paymentKey}";

            return new PaymentInitiationResult(true, paymentUrl, orderId.ToString(), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob payment initiation failed");
            return new PaymentInitiationResult(false, null, null, ex.Message);
        }
    }

    public Task<WebhookVerificationResult> VerifyWebhookAsync(string payload, string signature, CancellationToken ct = default)
    {
        // HMAC-SHA512 verification
        using var hmac = new System.Security.Cryptography.HMACSHA512(Encoding.UTF8.GetBytes(_hmacSecret));
        var computedHash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLower();
        var isValid = computedHash == signature?.ToLower();

        if (!isValid)
            return Task.FromResult(new WebhookVerificationResult(false, null, null, false, null));

        try
        {
            var doc = JsonDocument.Parse(payload);
            var obj = doc.RootElement.GetProperty("obj");
            var success = obj.GetProperty("success").GetBoolean();
            var transactionId = obj.GetProperty("id").GetInt64().ToString();
            var orderId = obj.GetProperty("order").GetProperty("id").GetInt64().ToString();
            var amount = obj.GetProperty("amount_cents").GetDecimal() / 100;

            return Task.FromResult(new WebhookVerificationResult(true, transactionId, orderId, success, amount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob webhook parse error");
            return Task.FromResult(new WebhookVerificationResult(false, null, null, false, null));
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(RefundRequest request, CancellationToken ct = default)
    {
        try
        {
            var authResponse = await _http.PostAsync("auth/tokens",
                new StringContent(JsonSerializer.Serialize(new { api_key = _apiKey }), Encoding.UTF8, "application/json"), ct);
            var authDoc = JsonDocument.Parse(await authResponse.Content.ReadAsStringAsync(ct));
            var token = authDoc.RootElement.GetProperty("token").GetString();

            var refundData = new
            {
                auth_token = token,
                transaction_id = request.TransactionId,
                amount_cents = (int)(request.Amount * 100)
            };

            var response = await _http.PostAsync("acceptance/void_refund/refund",
                new StringContent(JsonSerializer.Serialize(refundData), Encoding.UTF8, "application/json"), ct);

            var content = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(content);

            if (response.IsSuccessStatusCode)
            {
                var refundId = doc.RootElement.GetProperty("id").GetInt64().ToString();
                return new RefundResult(true, refundId, null);
            }

            return new RefundResult(false, null, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob refund failed");
            return new RefundResult(false, null, ex.Message);
        }
    }
}
