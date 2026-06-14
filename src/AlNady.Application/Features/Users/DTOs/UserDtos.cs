using AlNady.Domain.Enums;

namespace AlNady.Application.Features.Users.DTOs;

public record UserProfileDto(
    int UserId,
    string Email,
    string FullName,
    string? Phone,
    string? ProfileImage,
    string Role,
    bool IsVerified,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    string? NationalId
);

public record UpdateProfileRequest(
    string FullName,
    string? Phone,
    string? NationalId
);
