using AlNady.Domain.Enums;

namespace AlNady.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(int? userId, EventType eventType, string description, string? ipAddress = null, string? userAgent = null, object? additionalData = null, CancellationToken ct = default);
}
