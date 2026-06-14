using AlNady.Application.Features.Notifications.Commands;
using AlNady.Application.Features.Notifications.DTOs;
using AlNady.Application.Features.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlNady.API.Controllers;

[Route("api/notifications")]
[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly IMediator _mediator;
    public NotificationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get current user notifications.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool? unreadOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetMyNotificationsQuery(CurrentUserId, unreadOnly, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Get unread notification count.</summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUnreadCountQuery(CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Mark a single notification as read.</summary>
    [HttpPut("{id:int}/read")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new MarkNotificationReadCommand(id, CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Mark all notifications as read.</summary>
    [HttpPut("read-all")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        var result = await _mediator.Send(new MarkAllNotificationsReadCommand(CurrentUserId), ct);
        return ToActionResult(result);
    }
}
