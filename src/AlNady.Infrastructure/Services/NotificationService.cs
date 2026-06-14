using AlNady.Application.Interfaces;
using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using AlNady.Infrastructure.Hubs;
using AlNady.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AlNady.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ApplicationDbContext context,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendAsync(int userId, string title, string message, NotificationType type, string? actionUrl = null, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);

        // Push via SignalR
        try
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    notification.NotificationId,
                    notification.Title,
                    notification.Message,
                    notification.Type,
                    notification.ActionUrl,
                    notification.CreatedAt
                }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR push failed for user {UserId}", userId);
        }
    }

    public async Task SendToMultipleAsync(IEnumerable<int> userIds, string title, string message, NotificationType type, CancellationToken ct = default)
    {
        var userIdList = userIds.ToList();
        var notifications = userIdList.Select(uid => new Notification
        {
            UserId = uid,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(ct);

        var tasks = userIdList.Select(uid =>
            _hubContext.Clients.User(uid.ToString())
                .SendAsync("ReceiveNotification", new { title, message, type }, ct));
        await Task.WhenAll(tasks);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId, CancellationToken ct = default)
    {
        var notification = await _context.Notifications.FindAsync(new object[] { notificationId }, ct);
        if (notification != null && notification.UserId == userId)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken ct = default)
    {
        var notifications = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead);

        foreach (var n in notifications)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }
}
