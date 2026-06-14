using AlNady.Infrastructure.Persistence;
using AlNady.Domain.Entities;
using AlNady.Application.Interfaces;
using AlNady.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AlNady.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
        int? userId,
        EventType eventType,
        string description,
        string? ipAddress = null,
        string? userAgent = null,
        object? additionalData = null,
        CancellationToken ct = default)
    {
        var log = new EventLog
        {
            UserId = userId,
            EventType = eventType,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            AdditionalData = additionalData != null
                ? JsonSerializer.Serialize(additionalData)
                : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.EventLogs.Add(log);
        await _context.SaveChangesAsync(ct);
    }
}
