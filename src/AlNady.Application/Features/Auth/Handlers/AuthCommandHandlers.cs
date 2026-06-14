using AlNady.Application.Features.Auth.Commands;
using AlNady.Application.Features.Auth.DTOs;
using AlNady.Application.Interfaces;
using AlNady.Application.Interfaces.Repositories;
using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using AlNady.Application.Interfaces;
using AlNady.Shared.Common;
using AlNady.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace AlNady.Application.Features.Auth.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwt;
    private readonly IEmailService _email;
    private readonly IAuditService _audit;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(IApplicationDbContext context, IJwtService jwt, IEmailService email, IAuditService audit, ILogger<RegisterCommandHandler> logger)
    {
        _context = context;
        _jwt = jwt;
        _email = email;
        _audit = audit;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken ct)
    {
        // Check for duplicate email
        if (await _context.Users.AnyAsync(u => u.Email == request.Email.ToLower(), ct))
            return Result<AuthResponse>.Conflict("Email is already registered.");

        // Check for duplicate NationalId
        if (!string.IsNullOrEmpty(request.NationalId) &&
            await _context.Users.AnyAsync(u => u.NationalId == request.NationalId, ct))
            return Result<AuthResponse>.Conflict("National ID is already in use.");

        var user = new User
        {
            Email = request.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            Phone = request.Phone?.Trim(),
            NationalId = request.NationalId?.Trim(),
            Role = request.Role,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        // Create verification code
        var code = GenerateCode();
        _context.VerificationCodes.Add(new VerificationCode
        {
            UserId = user.UserId,
            Code = code,
            Type = "EmailVerification",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        });

        // Create refresh token session
        var refreshToken = _jwt.GenerateRefreshToken();
        _context.Sessions.Add(new Session
        {
            UserId = user.UserId,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(AppConstants.Jwt.RefreshTokenExpirationDays),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);

        // Send welcome + verification emails (fire-and-forget)
        _ = Task.Run(async () =>
        {
            await _email.SendWelcomeEmailAsync(user.Email, user.FullName);
            await _email.SendEmailVerificationAsync(user.Email, user.FullName, code);
        });

        await _audit.LogAsync(user.UserId, EventType.Register, $"User {user.Email} registered with role {user.Role}");

        var accessToken = _jwt.GenerateAccessToken(user.UserId, user.Email, user.Role.ToString());

        return Result<AuthResponse>.Success(new AuthResponse(
            user.UserId, user.Email, user.FullName, user.Role.ToString(),
            accessToken, refreshToken,
            DateTime.UtcNow.AddMinutes(AppConstants.Jwt.AccessTokenExpirationMinutes)
        ), 201);
    }

    private static string GenerateCode()
        => Random.Shared.Next(100000, 999999).ToString();
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwt;
    private readonly IAuditService _audit;

    public LoginCommandHandler(IApplicationDbContext context, IJwtService jwt, IAuditService audit)
    {
        _context = context;
        _jwt = jwt;
        _audit = audit;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await _audit.LogAsync(null, EventType.SecurityEvent, $"Failed login attempt for email: {request.Email}", request.IpAddress);
            return Result<AuthResponse>.Unauthorized("Invalid email or password.");
        }

        // Check blacklist
        var isBlacklisted = await _context.Blacklists
            .AnyAsync(b => b.UserId == user.UserId && b.IsActive, ct);
        if (isBlacklisted)
            return Result<AuthResponse>.Forbidden("Your account has been suspended. Contact support.");

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;

        // Create new session
        var refreshToken = _jwt.GenerateRefreshToken();

        // Deactivate old sessions over limit (keep last 5)
        var activeSessions = await _context.Sessions
            .Where(s => s.UserId == user.UserId && s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        if (activeSessions.Count >= 5)
        {
            foreach (var old in activeSessions.Skip(4))
                old.IsActive = false;
        }

        _context.Sessions.Add(new Session
        {
            UserId = user.UserId,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(AppConstants.Jwt.RefreshTokenExpirationDays),
            IsActive = true,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(user.UserId, EventType.Login, "User logged in", request.IpAddress, request.UserAgent);

        var accessToken = _jwt.GenerateAccessToken(user.UserId, user.Email, user.Role.ToString());

        return Result<AuthResponse>.Success(new AuthResponse(
            user.UserId, user.Email, user.FullName, user.Role.ToString(),
            accessToken, refreshToken,
            DateTime.UtcNow.AddMinutes(AppConstants.Jwt.AccessTokenExpirationMinutes)
        ));
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwt;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var session = await _context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshToken == request.RefreshToken && s.IsActive, ct);

        if (session == null || session.ExpiresAt < DateTime.UtcNow)
            return Result<TokenResponse>.Unauthorized("Invalid or expired refresh token.");

        // Rotate refresh token
        session.IsActive = false;
        var newRefreshToken = _jwt.GenerateRefreshToken();

        _context.Sessions.Add(new Session
        {
            UserId = session.UserId,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(AppConstants.Jwt.RefreshTokenExpirationDays),
            IsActive = true,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);

        var newAccessToken = _jwt.GenerateAccessToken(session.UserId, session.User.Email, session.User.Role.ToString());

        return Result<TokenResponse>.Success(new TokenResponse(
            newAccessToken, newRefreshToken,
            DateTime.UtcNow.AddMinutes(AppConstants.Jwt.AccessTokenExpirationMinutes)
        ));
    }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _audit;

    public LogoutCommandHandler(IApplicationDbContext context, IAuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.RefreshToken == request.RefreshToken && s.IsActive, ct);

        if (session != null)
        {
            session.IsActive = false;
            await _context.SaveChangesAsync(ct);
            await _audit.LogAsync(session.UserId, EventType.Logout, "User logged out");
        }

        return Result.Success();
    }
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public VerifyEmailCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct);
        if (user == null) return Result.Failure("User not found.", 404);
        if (user.IsVerified) return Result.Failure("Email is already verified.");

        var code = await _context.VerificationCodes
            .FirstOrDefaultAsync(v => v.UserId == user.UserId &&
                                      v.Code == request.Code &&
                                      v.Type == "EmailVerification" &&
                                      !v.IsUsed &&
                                      v.ExpiresAt > DateTime.UtcNow, ct);

        if (code == null) return Result.Failure("Invalid or expired verification code.");

        user.IsVerified = true;
        code.IsUsed = true;
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _email;

    public ForgotPasswordCommandHandler(IApplicationDbContext context, IEmailService email)
    {
        _context = context;
        _email = email;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct);

        // Always return success to prevent email enumeration
        if (user == null) return Result.Success();

        var code = Random.Shared.Next(100000, 999999).ToString();

        // Invalidate old codes
        var oldCodes = _context.VerificationCodes
            .Where(v => v.UserId == user.UserId && v.Type == "PasswordReset" && !v.IsUsed);
        foreach (var old in oldCodes) old.IsUsed = true;

        _context.VerificationCodes.Add(new VerificationCode
        {
            UserId = user.UserId,
            Code = code,
            Type = "PasswordReset",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        });

        await _context.SaveChangesAsync(ct);

        _ = Task.Run(() => _email.SendPasswordResetAsync(user.Email, user.FullName, code));

        return Result.Success();
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _audit;

    public ResetPasswordCommandHandler(IApplicationDbContext context, IAuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct);
        if (user == null) return Result.Failure("User not found.", 404);

        var code = await _context.VerificationCodes
            .FirstOrDefaultAsync(v => v.UserId == user.UserId &&
                                      v.Code == request.Code &&
                                      v.Type == "PasswordReset" &&
                                      !v.IsUsed &&
                                      v.ExpiresAt > DateTime.UtcNow, ct);

        if (code == null) return Result.Failure("Invalid or expired reset code.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        code.IsUsed = true;

        // Invalidate all sessions
        var sessions = _context.Sessions.Where(s => s.UserId == user.UserId && s.IsActive);
        foreach (var s in sessions) s.IsActive = false;

        await _context.SaveChangesAsync(ct);
        await _audit.LogAsync(user.UserId, EventType.PasswordReset, "Password reset successfully");

        return Result.Success();
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _audit;

    public ChangePasswordCommandHandler(IApplicationDbContext context, IAuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, ct);
        if (user == null) return Result.Failure("User not found.", 404);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return Result.Unauthorized("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync(ct);
        await _audit.LogAsync(user.UserId, EventType.PasswordChange, "Password changed");

        return Result.Success();
    }
}
