using AlNady.Domain.Enums;

namespace AlNady.Application.Features.Auth.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    string? Phone,
    UserRole Role,
    string? NationalId
);

public record LoginRequest(
    string Email,
    string Password
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record VerifyEmailRequest(
    string Email,
    string Code
);

public record ForgotPasswordRequest(
    string Email
);

public record ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

public record AuthResponse(
    int UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry
);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry
);
