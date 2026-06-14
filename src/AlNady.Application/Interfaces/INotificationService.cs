using AlNady.Domain.Enums;

namespace AlNady.Application.Interfaces;

public interface INotificationService
{
    Task SendAsync(int userId, string title, string message, NotificationType type, string? actionUrl = null, CancellationToken ct = default);
    Task SendToMultipleAsync(IEnumerable<int> userIds, string title, string message, NotificationType type, CancellationToken ct = default);
    Task MarkAsReadAsync(int notificationId, int userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(int userId, CancellationToken ct = default);
}
