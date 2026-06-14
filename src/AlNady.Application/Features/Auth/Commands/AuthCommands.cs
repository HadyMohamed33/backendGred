using AlNady.Application.Features.Auth.DTOs;
using AlNady.Shared.Common;
using MediatR;

namespace AlNady.Application.Features.Auth.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string? Phone,
    AlNady.Domain.Enums.UserRole Role,
    string? NationalId
) : IRequest<Result<AuthResponse>>;

public record LoginCommand(
    string Email,
    string Password,
    string? IpAddress,
    string? UserAgent
) : IRequest<Result<AuthResponse>>;

public record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress,
    string? UserAgent
) : IRequest<Result<TokenResponse>>;

public record LogoutCommand(
    string RefreshToken
) : IRequest<Result>;

public record VerifyEmailCommand(
    string Email,
    string Code
) : IRequest<Result>;

public record ForgotPasswordCommand(
    string Email
) : IRequest<Result>;

public record ResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword
) : IRequest<Result>;

public record ChangePasswordCommand(
    int UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<Result>;
