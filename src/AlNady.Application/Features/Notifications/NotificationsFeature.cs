using AlNady.Application.Interfaces;
using AlNady.Domain.Enums;
using AlNady.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlNady.Application.Features.Notifications.DTOs
{
    public record NotificationDto(int NotificationId, string? Title, string Message, string Type, bool IsRead, string? ActionUrl, DateTime CreatedAt, DateTime? ReadAt);
}

namespace AlNady.Application.Features.Notifications.Queries
{
    using DTOs;
    public record GetMyNotificationsQuery(int UserId, bool? UnreadOnly, int Page, int PageSize) : IRequest<Result<PagedResult<NotificationDto>>>;
    public record GetUnreadCountQuery(int UserId) : IRequest<Result<int>>;
}

namespace AlNady.Application.Features.Notifications.Commands
{
    public record MarkNotificationReadCommand(int NotificationId, int UserId) : IRequest<Result>;
    public record MarkAllNotificationsReadCommand(int UserId) : IRequest<Result>;
}

namespace AlNady.Application.Features.Notifications.Handlers
{
    using DTOs;

    public class GetMyNotificationsQueryHandler : IRequestHandler<Queries.GetMyNotificationsQuery, Result<PagedResult<NotificationDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetMyNotificationsQueryHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result<PagedResult<NotificationDto>>> Handle(Queries.GetMyNotificationsQuery request, CancellationToken ct)
        {
            var query = _context.Notifications.AsNoTracking().Where(n => n.UserId == request.UserId);
            if (request.UnreadOnly == true) query = query.Where(n => !n.IsRead);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(n => n.CreatedAt).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            var dtos = items.Select(n => new NotificationDto(n.NotificationId, n.Title, n.Message, n.Type.ToString(), n.IsRead, n.ActionUrl, n.CreatedAt, n.ReadAt)).ToList();
            return Result<PagedResult<NotificationDto>>.Success(PagedResult<NotificationDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }

    public class GetUnreadCountQueryHandler : IRequestHandler<Queries.GetUnreadCountQuery, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        public GetUnreadCountQueryHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result<int>> Handle(Queries.GetUnreadCountQuery request, CancellationToken ct)
        {
            var count = await _context.Notifications.CountAsync(n => n.UserId == request.UserId && !n.IsRead, ct);
            return Result<int>.Success(count);
        }
    }

    public class MarkNotificationReadCommandHandler : IRequestHandler<Commands.MarkNotificationReadCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        public MarkNotificationReadCommandHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result> Handle(Commands.MarkNotificationReadCommand request, CancellationToken ct)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == request.NotificationId && n.UserId == request.UserId, ct);
            if (notification == null) return Result.NotFound("Notification not found.");
            notification.IsRead = true; notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
    }

    public class MarkAllNotificationsReadCommandHandler : IRequestHandler<Commands.MarkAllNotificationsReadCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        public MarkAllNotificationsReadCommandHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result> Handle(Commands.MarkAllNotificationsReadCommand request, CancellationToken ct)
        {
            var notifications = await _context.Notifications.Where(n => n.UserId == request.UserId && !n.IsRead).ToListAsync(ct);
            foreach (var n in notifications) { n.IsRead = true; n.ReadAt = DateTime.UtcNow; }
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}
