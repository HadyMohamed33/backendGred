using AlNady.Application.Interfaces;
using AlNady.Domain.Enums;
using AlNady.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlNady.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring jobs for background processing.
/// </summary>
public class NotificationCleanupJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationCleanupJob> _logger;

    public NotificationCleanupJob(ApplicationDbContext context, ILogger<NotificationCleanupJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupOldNotificationsAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var old = await _context.Notifications
            .Where(n => n.IsRead && n.CreatedAt < cutoff)
            .ToListAsync();

        _context.Notifications.RemoveRange(old);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleaned up {Count} old notifications", old.Count);
    }
}

public class ProgramStatusUpdateJob
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notifications;
    private readonly ILogger<ProgramStatusUpdateJob> _logger;

    public ProgramStatusUpdateJob(ApplicationDbContext context, INotificationService notifications, ILogger<ProgramStatusUpdateJob> logger)
    {
        _context = context;
        _notifications = notifications;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task UpdateProgramStatusesAsync()
    {
        var now = DateTime.UtcNow;

        // Mark programs as Ongoing
        var startingPrograms = await _context.TrainingPrograms
            .Where(p => p.Status == ProgramStatus.Published && p.ProgramDate <= now)
            .ToListAsync();

        foreach (var p in startingPrograms)
            p.Status = ProgramStatus.Ongoing;

        // Mark programs as Completed (assume 3-hour sessions)
        var endingPrograms = await _context.TrainingPrograms
            .Where(p => p.Status == ProgramStatus.Ongoing &&
                        p.ProgramDate.AddHours(3) < now)
            .ToListAsync();

        foreach (var p in endingPrograms)
            p.Status = ProgramStatus.Completed;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated {Start} to Ongoing, {End} to Completed",
            startingPrograms.Count, endingPrograms.Count);
    }
}

public class ExpiredSessionCleanupJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExpiredSessionCleanupJob> _logger;

    public ExpiredSessionCleanupJob(ApplicationDbContext context, ILogger<ExpiredSessionCleanupJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupExpiredSessionsAsync()
    {
        var expired = await _context.Sessions
            .Where(s => s.ExpiresAt < DateTime.UtcNow || !s.IsActive)
            .ToListAsync();

        _context.Sessions.RemoveRange(expired);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed {Count} expired sessions", expired.Count);
    }
}

public class ExpiredVerificationCodeCleanupJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExpiredVerificationCodeCleanupJob> _logger;

    public ExpiredVerificationCodeCleanupJob(ApplicationDbContext context, ILogger<ExpiredVerificationCodeCleanupJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupExpiredCodesAsync()
    {
        var expired = await _context.VerificationCodes
            .Where(v => v.ExpiresAt < DateTime.UtcNow || v.IsUsed)
            .ToListAsync();

        _context.VerificationCodes.RemoveRange(expired);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed {Count} expired verification codes", expired.Count);
    }
}

/// <summary>
/// Registers all recurring Hangfire jobs.
/// </summary>
public static class HangfireJobScheduler
{
    public static void RegisterRecurringJobs()
    {
        RecurringJob.AddOrUpdate<NotificationCleanupJob>(
            "cleanup-old-notifications",
            j => j.CleanupOldNotificationsAsync(),
            Cron.Daily(2)); // 2am daily

        RecurringJob.AddOrUpdate<ProgramStatusUpdateJob>(
            "update-program-statuses",
            j => j.UpdateProgramStatusesAsync(),
            Cron.Minutely()); // every minute

        RecurringJob.AddOrUpdate<ExpiredSessionCleanupJob>(
            "cleanup-expired-sessions",
            j => j.CleanupExpiredSessionsAsync(),
            Cron.Daily(3)); // 3am daily

        RecurringJob.AddOrUpdate<ExpiredVerificationCodeCleanupJob>(
            "cleanup-expired-codes",
            j => j.CleanupExpiredCodesAsync(),
            Cron.Hourly()); // every hour
    }
}
