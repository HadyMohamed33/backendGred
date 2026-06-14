using AlNady.Application.Features.Admin.DTOs;
using AlNady.Application.Features.Academies.DTOs;
using AlNady.Application.Features.Trainers.DTOs;
using AlNady.Application.Interfaces;
using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using AlNady.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlNady.Application.Features.Admin.DTOs
{
    public record DashboardStatsDto(int TotalUsers, int TotalTrainers, int TotalAcademies, int TotalPrograms, int TotalEnrollments, decimal TotalRevenue, int PendingTrainerApprovals, int PendingAcademyApprovals, int ActiveBlacklists, int UnreadNotifications);
    public record AuditLogDto(int LogId, int? UserId, string? UserEmail, string EventType, string Description, string? IpAddress, DateTime CreatedAt);
    public record BlacklistDto(int BlacklistId, int UserId, string UserEmail, string UserName, string Reason, int BlacklistedByAdminId, DateTime CreatedAt, bool IsActive);
}

namespace AlNady.Application.Features.Admin.Commands
{
    public record ApproveTrainerCommand(int TrainerId, int AdminUserId) : IRequest<Result>;
    public record RejectTrainerCommand(int TrainerId, int AdminUserId, string Reason) : IRequest<Result>;
    public record ApproveAcademyCommand(int AcademyId, int AdminUserId) : IRequest<Result>;
    public record RejectAcademyCommand(int AcademyId, int AdminUserId, string Reason) : IRequest<Result>;
    public record BlacklistUserCommand(int TargetUserId, int AdminUserId, string Reason) : IRequest<Result>;
    public record RemoveFromBlacklistCommand(int TargetUserId, int AdminUserId) : IRequest<Result>;
    public record VerifyCertificateCommand(int CertificateId, int AdminUserId) : IRequest<Result>;
}

namespace AlNady.Application.Features.Admin.Queries
{
    public record GetDashboardStatsQuery(int AdminUserId) : IRequest<Result<DashboardStatsDto>>;
    public record GetAuditLogsQuery(int Page, int PageSize, string? EventType, int? UserId, DateTime? From, DateTime? To) : IRequest<Result<PagedResult<AuditLogDto>>>;
    public record GetBlacklistQuery(int Page, int PageSize, bool ActiveOnly) : IRequest<Result<PagedResult<BlacklistDto>>>;
    public record GetPendingTrainersQuery(int Page, int PageSize) : IRequest<Result<PagedResult<TrainerProfileDto>>>;
    public record GetPendingAcademiesQuery(int Page, int PageSize) : IRequest<Result<PagedResult<AcademyProfileDto>>>;
}

namespace AlNady.Application.Features.Admin.Handlers
{
    public class ApproveTrainerCommandHandler : IRequestHandler<Commands.ApproveTrainerCommand, Result>
    {
        private readonly IApplicationDbContext _context; private readonly INotificationService _notifications; private readonly IAuditService _audit;
        public ApproveTrainerCommandHandler(IApplicationDbContext context, INotificationService notifications, IAuditService audit) { _context = context; _notifications = notifications; _audit = audit; }
        public async Task<Result> Handle(Commands.ApproveTrainerCommand request, CancellationToken ct)
        {
            var trainer = await _context.Trainers.FindAsync(new object[] { request.TrainerId }, ct);
            if (trainer == null) return Result.NotFound("Trainer not found.");
            if (trainer.IsVerifiedByAdmin) return Result.Failure("Trainer already verified.");
            trainer.IsVerifiedByAdmin = true;
            await _context.SaveChangesAsync(ct);
            await _notifications.SendAsync(trainer.UserId, "Profile Approved", "Your trainer profile has been approved. You can now create programs.", NotificationType.AdminAction, ct: ct);
            await _audit.LogAsync(request.AdminUserId, EventType.AdminApprovedTrainer, $"Admin approved trainer {request.TrainerId}", ct: ct);
            return Result.Success();
        }
    }

    public class RejectTrainerCommandHandler : IRequestHandler<Commands.RejectTrainerCommand, Result>
    {
        private readonly IApplicationDbContext _context; private readonly INotificationService _notifications; private readonly IAuditService _audit;
        public RejectTrainerCommandHandler(IApplicationDbContext context, INotificationService notifications, IAuditService audit) { _context = context; _notifications = notifications; _audit = audit; }
        public async Task<Result> Handle(Commands.RejectTrainerCommand request, CancellationToken ct)
        {
            var trainer = await _context.Trainers.FindAsync(new object[] { request.TrainerId }, ct);
            if (trainer == null) return Result.NotFound("Trainer not found.");
            trainer.IsVerifiedByAdmin = false;
            await _context.SaveChangesAsync(ct);
            await _notifications.SendAsync(trainer.UserId, "Profile Rejected", $"Your trainer profile was rejected. Reason: {request.Reason}", NotificationType.AdminAction, ct: ct);
            await _audit.LogAsync(request.AdminUserId, EventType.AdminRejectedTrainer, $"Admin rejected trainer {request.TrainerId}", ct: ct);
            return Result.Success();
        }
    }

    public class ApproveAcademyCommandHandler : IRequestHandler<Commands.ApproveAcademyCommand, Result>
    {
        private readonly IApplicationDbContext _context; private readonly INotificationService _notifications; private readonly IAuditService _audit;
        public ApproveAcademyCommandHandler(IApplicationDbContext context, INotificationService notifications, IAuditService audit) { _context = context; _notifications = notifications; _audit = audit; }
        public async Task<Result> Handle(Commands.ApproveAcademyCommand request, CancellationToken ct)
        {
            var academy = await _context.Academies.FindAsync(new object[] { request.AcademyId }, ct);
            if (academy == null) return Result.NotFound("Academy not found.");
            if (academy.IsVerified) return Result.Failure("Academy already verified.");
            academy.IsVerified = true;
            await _context.SaveChangesAsync(ct);
            await _notifications.SendAsync(academy.UserId, "Academy Approved", "Your academy profile has been approved.", NotificationType.AdminAction, ct: ct);
            await _audit.LogAsync(request.AdminUserId, EventType.AdminApprovedAcademy, $"Admin approved academy {request.AcademyId}", ct: ct);
            return Result.Success();
        }
    }

    public class RejectAcademyCommandHandler : IRequestHandler<Commands.RejectAcademyCommand, Result>
    {
        private readonly IApplicationDbContext _context; private readonly INotificationService _notifications; private readonly IAuditService _audit;
        public RejectAcademyCommandHandler(IApplicationDbContext context, INotificationService notifications, IAuditService audit) { _context = context; _notifications = notifications; _audit = audit; }
        public async Task<Result> Handle(Commands.RejectAcademyCommand request, CancellationToken ct)
        {
            var academy = await _context.Academies.FindAsync(new object[] { request.AcademyId }, ct);
            if (academy == null) return Result.NotFound("Academy not found.");
            academy.IsVerified = false;
            await _context.SaveChangesAsync(ct);
            await _notifications.SendAsync(academy.UserId, "Academy Rejected", $"Your academy profile was rejected. Reason: {request.Reason}", NotificationType.AdminAction, ct: ct);
            await _audit.LogAsync(request.AdminUserId, EventType.AdminRejectedAcademy, $"Admin rejected academy {request.AcademyId}", ct: ct);
            return Result.Success();
        }
    }

    public class BlacklistUserCommandHandler : IRequestHandler<Commands.BlacklistUserCommand, Result>
    {
        private readonly IApplicationDbContext _context; private readonly IAuditService _audit;
        public BlacklistUserCommandHandler(IApplicationDbContext context, IAuditService audit) { _context = context; _audit = audit; }
        public async Task<Result> Handle(Commands.BlacklistUserCommand request, CancellationToken ct)
        {
            var user = await _context.Users.FindAsync(new object[] { request.TargetUserId }, ct);
            if (user == null) return Result.NotFound("User not found.");
            if (user.Role == UserRole.Admin) return Result.Failure("Cannot blacklist an admin.");
            var existing = await _context.Blacklists.FirstOrDefaultAsync(b => b.UserId == request.TargetUserId && b.IsActive, ct);
            if (existing != null) return Result.Conflict("User is already blacklisted.");
            _context.Blacklists.Add(new Blacklist { UserId = request.TargetUserId, Reason = request.Reason, BlacklistedByAdminId = request.AdminUserId, IsActive = true, CreatedAt = DateTime.UtcNow });
            var sessions = _context.Sessions.Where(s => s.UserId == request.TargetUserId && s.IsActive);
            foreach (var s in sessions) s.IsActive = false;
            await _context.SaveChangesAsync(ct);
            await _audit.LogAsync(request.AdminUserId, EventType.UserBlacklisted, $"User {request.TargetUserId} blacklisted. Reason: {request.Reason}", ct: ct);
            return Result.Success();
        }
    }

    public class RemoveFromBlacklistCommandHandler : IRequestHandler<Commands.RemoveFromBlacklistCommand, Result>
    {
        private readonly IApplicationDbContext _context; private readonly INotificationService _notifications; private readonly IAuditService _audit;
        public RemoveFromBlacklistCommandHandler(IApplicationDbContext context, INotificationService notifications, IAuditService audit) { _context = context; _notifications = notifications; _audit = audit; }
        public async Task<Result> Handle(Commands.RemoveFromBlacklistCommand request, CancellationToken ct)
        {
            var blacklist = await _context.Blacklists.FirstOrDefaultAsync(b => b.UserId == request.TargetUserId && b.IsActive, ct);
            if (blacklist == null) return Result.NotFound("Active blacklist entry not found.");
            blacklist.IsActive = false;
            await _context.SaveChangesAsync(ct);
            await _notifications.SendAsync(request.TargetUserId, "Account Reinstated", "Your account suspension has been lifted.", NotificationType.AdminAction, ct: ct);
            await _audit.LogAsync(request.AdminUserId, EventType.UserRemovedFromBlacklist, $"User {request.TargetUserId} removed from blacklist", ct: ct);
            return Result.Success();
        }
    }

    public class GetDashboardStatsQueryHandler : IRequestHandler<Queries.GetDashboardStatsQuery, Result<DashboardStatsDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetDashboardStatsQueryHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result<DashboardStatsDto>> Handle(Queries.GetDashboardStatsQuery request, CancellationToken ct)
        {
            var totalUsers = await _context.Users.CountAsync(ct);
            var totalTrainers = await _context.Trainers.CountAsync(ct);
            var totalAcademies = await _context.Academies.CountAsync(ct);
            var totalPrograms = await _context.TrainingPrograms.CountAsync(ct);
            var totalEnrollments = await _context.FormResponses.CountAsync(ct);
            var totalRevenue = await _context.Payments.Where(p => p.Status == PaymentStatus.Completed).SumAsync(p => p.Amount, ct);
            var pendingTrainers = await _context.Trainers.CountAsync(t => !t.IsVerifiedByAdmin, ct);
            var pendingAcademies = await _context.Academies.CountAsync(a => !a.IsVerified, ct);
            var activeBlacklists = await _context.Blacklists.CountAsync(b => b.IsActive, ct);
            return Result<DashboardStatsDto>.Success(new DashboardStatsDto(totalUsers, totalTrainers, totalAcademies, totalPrograms, totalEnrollments, totalRevenue, pendingTrainers, pendingAcademies, activeBlacklists, 0));
        }
    }

    public class GetAuditLogsQueryHandler : IRequestHandler<Queries.GetAuditLogsQuery, Result<PagedResult<AuditLogDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetAuditLogsQueryHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result<PagedResult<AuditLogDto>>> Handle(Queries.GetAuditLogsQuery request, CancellationToken ct)
        {
            var query = _context.EventLogs.AsNoTracking().Include(e => e.User).AsQueryable();
            if (!string.IsNullOrEmpty(request.EventType) && Enum.TryParse<EventType>(request.EventType, true, out var et)) query = query.Where(e => e.EventType == et);
            if (request.UserId.HasValue) query = query.Where(e => e.UserId == request.UserId.Value);
            if (request.From.HasValue) query = query.Where(e => e.CreatedAt >= request.From.Value);
            if (request.To.HasValue) query = query.Where(e => e.CreatedAt <= request.To.Value);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(e => e.CreatedAt).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            var dtos = items.Select(e => new AuditLogDto(e.LogId, e.UserId, e.User?.Email, e.EventType.ToString(), e.Description, e.IpAddress, e.CreatedAt)).ToList();
            return Result<PagedResult<AuditLogDto>>.Success(PagedResult<AuditLogDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }

    public class GetBlacklistQueryHandler : IRequestHandler<Queries.GetBlacklistQuery, Result<PagedResult<BlacklistDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetBlacklistQueryHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result<PagedResult<BlacklistDto>>> Handle(Queries.GetBlacklistQuery request, CancellationToken ct)
        {
            var query = _context.Blacklists.AsNoTracking().Include(b => b.User).AsQueryable();
            if (request.ActiveOnly) query = query.Where(b => b.IsActive);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(b => b.CreatedAt).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            var dtos = items.Select(b => new BlacklistDto(b.BlacklistId, b.UserId, b.User.Email, b.User.FullName, b.Reason, b.BlacklistedByAdminId, b.CreatedAt, b.IsActive)).ToList();
            return Result<PagedResult<BlacklistDto>>.Success(PagedResult<BlacklistDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }

    public class GetPendingTrainersQueryHandler : IRequestHandler<Queries.GetPendingTrainersQuery, Result<PagedResult<TrainerProfileDto>>>
    {
        private readonly IApplicationDbContext _context; private readonly IFileStorageService _storage;
        public GetPendingTrainersQueryHandler(IApplicationDbContext context, IFileStorageService storage) { _context = context; _storage = storage; }
        public async Task<Result<PagedResult<TrainerProfileDto>>> Handle(Queries.GetPendingTrainersQuery request, CancellationToken ct)
        {
            var query = _context.Trainers.AsNoTracking().Include(t => t.User).Include(t => t.Certificates).Where(t => !t.IsVerifiedByAdmin);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(t => t.TrainerId).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            var dtos = items.Select(t => new TrainerProfileDto(t.TrainerId, t.UserId, t.User.FullName, t.User.Email, t.User.ProfileImage, t.About, t.SpecializationSports, t.IsVerifiedByAdmin, t.AverageRating, t.AgeCategory, t.GenderPreference,
                t.Certificates.Select(c => new CertificateDto(c.CertificateId, c.CertificateName, _storage.GetPublicUrl(c.FilePath), c.IsVerifiedByAdmin, c.DateAdded)).ToList())).ToList();
            return Result<PagedResult<TrainerProfileDto>>.Success(PagedResult<TrainerProfileDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }

    public class GetPendingAcademiesQueryHandler : IRequestHandler<Queries.GetPendingAcademiesQuery, Result<PagedResult<AcademyProfileDto>>>
    {
        private readonly IApplicationDbContext _context; private readonly IFileStorageService _storage;
        public GetPendingAcademiesQueryHandler(IApplicationDbContext context, IFileStorageService storage) { _context = context; _storage = storage; }
        public async Task<Result<PagedResult<AcademyProfileDto>>> Handle(Queries.GetPendingAcademiesQuery request, CancellationToken ct)
        {
            var query = _context.Academies.AsNoTracking().Include(a => a.User).Include(a => a.Certificates).Where(a => !a.IsVerified);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(a => a.AcademyId).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            var dtos = items.Select(a => new AcademyProfileDto(a.AcademyId, a.UserId, a.User.FullName, a.User.Email, a.User.ProfileImage, a.SpecializationSports, a.IsVerified, a.AverageRating, a.Location, a.AgeCategory, a.GenderPreference,
                a.Certificates.Select(c => new CertificateDto(c.CertificateId, c.CertificateName, _storage.GetPublicUrl(c.FilePath), c.IsVerifiedByAdmin, c.DateAdded)).ToList())).ToList();
            return Result<PagedResult<AcademyProfileDto>>.Success(PagedResult<AcademyProfileDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }

    public class VerifyCertificateCommandHandler : IRequestHandler<Commands.VerifyCertificateCommand, Result>
    {
        private readonly IApplicationDbContext _context; private readonly IAuditService _audit;
        public VerifyCertificateCommandHandler(IApplicationDbContext context, IAuditService audit) { _context = context; _audit = audit; }
        public async Task<Result> Handle(Commands.VerifyCertificateCommand request, CancellationToken ct)
        {
            var cert = await _context.Certificates.FindAsync(new object[] { request.CertificateId }, ct);
            if (cert == null) return Result.NotFound("Certificate not found.");
            cert.IsVerifiedByAdmin = true;
            await _context.SaveChangesAsync(ct);
            await _audit.LogAsync(request.AdminUserId, EventType.CertificateUploaded, $"Admin verified certificate {request.CertificateId}", ct: ct);
            return Result.Success();
        }
    }
}
