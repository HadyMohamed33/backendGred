using AlNady.Application.Features.Admin.Commands;
using AlNady.Application.Features.Admin.DTOs;
using AlNady.Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlNady.API.Controllers;

[Route("api/admin")]
[Authorize(Policy = "RequireAdmin")]
public class AdminController : BaseApiController
{
    private readonly IMediator _mediator;
    public AdminController(IMediator mediator) => _mediator = mediator;

    // ─── Dashboard ────────────────────────────────────────────────────────────

    /// <summary>Get platform dashboard statistics.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardStatsDto), 200)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardStatsQuery(CurrentUserId), ct);
        return ToActionResult(result);
    }

    // ─── Trainer Approval ─────────────────────────────────────────────────────

    /// <summary>Get pending trainer approvals.</summary>
    [HttpGet("trainers/pending")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetPendingTrainers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPendingTrainersQuery(page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Approve a trainer profile.</summary>
    [HttpPost("trainers/{trainerId:int}/approve")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ApproveTrainer(int trainerId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveTrainerCommand(trainerId, CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Reject a trainer profile.</summary>
    [HttpPost("trainers/{trainerId:int}/reject")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RejectTrainer(int trainerId, [FromBody] RejectReasonRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RejectTrainerCommand(trainerId, CurrentUserId, request.Reason), ct);
        return ToActionResult(result);
    }

    // ─── Academy Approval ─────────────────────────────────────────────────────

    /// <summary>Get pending academy approvals.</summary>
    [HttpGet("academies/pending")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetPendingAcademies(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPendingAcademiesQuery(page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Approve an academy profile.</summary>
    [HttpPost("academies/{academyId:int}/approve")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ApproveAcademy(int academyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveAcademyCommand(academyId, CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Reject an academy profile.</summary>
    [HttpPost("academies/{academyId:int}/reject")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RejectAcademy(int academyId, [FromBody] RejectReasonRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RejectAcademyCommand(academyId, CurrentUserId, request.Reason), ct);
        return ToActionResult(result);
    }

    // ─── Blacklist ────────────────────────────────────────────────────────────

    /// <summary>Get blacklisted users.</summary>
    [HttpGet("blacklist")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetBlacklist(
        [FromQuery] bool activeOnly = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetBlacklistQuery(page, pageSize, activeOnly), ct);
        return ToActionResult(result);
    }

    /// <summary>Blacklist a user.</summary>
    [HttpPost("users/{userId:int}/blacklist")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> BlacklistUser(int userId, [FromBody] RejectReasonRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new BlacklistUserCommand(userId, CurrentUserId, request.Reason), ct);
        return ToActionResult(result);
    }

    /// <summary>Remove user from blacklist.</summary>
    [HttpDelete("users/{userId:int}/blacklist")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveFromBlacklist(int userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveFromBlacklistCommand(userId, CurrentUserId), ct);
        return ToActionResult(result);
    }

    // ─── Audit Logs ───────────────────────────────────────────────────────────

    /// <summary>View audit logs with filters.</summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? eventType,
        [FromQuery] int? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAuditLogsQuery(page, pageSize, eventType, userId, from, to), ct);
        return ToActionResult(result);
    }

    // ─── Certificate Verification ─────────────────────────────────────────────

    /// <summary>Verify an uploaded certificate.</summary>
    [HttpPost("certificates/{certificateId:int}/verify")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> VerifyCertificate(int certificateId, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyCertificateCommand(certificateId, CurrentUserId), ct);
        return ToActionResult(result);
    }

    // ─── Cancel Enrollment (Admin) ────────────────────────────────────────────

    /// <summary>Cancel an enrollment on behalf of a user (admin).</summary>
    [HttpPost("enrollments/{responseId:int}/cancel")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> AdminCancelEnrollment(int responseId, [FromBody] RejectReasonRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AlNady.Application.Features.Payments.Commands.CancelEnrollmentCommand(
                responseId, CurrentUserId, request.Reason, AlNady.Domain.Enums.CancelledBy.Admin), ct);
        return ToActionResult(result);
    }
}

public record RejectReasonRequest(string Reason);
