using AlNady.Application.Features.Payments.Commands;
using AlNady.Application.Features.Payments.DTOs;
using AlNady.Application.Features.Payments.Queries;
using AlNady.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlNady.API.Controllers;

[Route("api/payments")]
[Authorize]
public class PaymentsController : BaseApiController
{
    private readonly IMediator _mediator;
    public PaymentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Initiate payment for an enrollment.</summary>
    [HttpPost("initiate")]
    [ProducesResponseType(typeof(InitiatePaymentResultDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new InitiatePaymentCommand(request.ResponseId, CurrentUserId, request.Method), ct);
        return ToActionResult(result);
    }

    /// <summary>Get payment status for an enrollment.</summary>
    [HttpGet("status/{responseId:int}")]
    [ProducesResponseType(typeof(PaymentDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPaymentStatus(int responseId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPaymentStatusQuery(responseId, CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Cancel an enrollment and process refund.</summary>
    [HttpPost("cancel/{responseId:int}")]
    [ProducesResponseType(typeof(CancellationDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CancelEnrollment(int responseId, [FromBody] CancelEnrollmentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CancelEnrollmentCommand(responseId, CurrentUserId, request.Reason, CancelledBy.Player), ct);
        return ToActionResult(result);
    }

    // ─── Webhooks (unauthenticated) ───────────────────────────────────────────

    /// <summary>Paymob payment webhook callback.</summary>
    [HttpPost("webhooks/paymob")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> PaymobWebhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(ct);
        var signature = Request.Headers["HMAC"].FirstOrDefault() ?? string.Empty;

        var result = await _mediator.Send(new ProcessWebhookCommand("Paymob", payload, signature), ct);
        return result.IsSuccess ? Ok() : BadRequest();
    }

    /// <summary>Stripe payment webhook callback.</summary>
    [HttpPost("webhooks/stripe")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> StripeWebhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

        var result = await _mediator.Send(new ProcessWebhookCommand("Stripe", payload, signature), ct);
        return result.IsSuccess ? Ok() : BadRequest();
    }
}
