using AlNady.Application.Features.Users.Commands;
using AlNady.Application.Features.Users.DTOs;
using AlNady.Application.Features.Users.Queries;
using AlNady.Application.Interfaces;
using AlNady.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlNady.Application.Features.Users.Handlers
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserProfileDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetCurrentUserQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<Result<UserProfileDto>> Handle(GetCurrentUserQuery request, CancellationToken ct)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, ct);
            if (user == null) return Result<UserProfileDto>.NotFound("User not found.");
            return Result<UserProfileDto>.Success(new UserProfileDto(
                user.UserId, user.Email, user.FullName, user.Phone,
                user.ProfileImage, user.Role.ToString(), user.IsVerified,
                user.CreatedAt, user.LastLoginAt, user.NationalId));
        }
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserProfileDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetUserByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<Result<UserProfileDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, ct);
            if (user == null) return Result<UserProfileDto>.NotFound("User not found.");
            return Result<UserProfileDto>.Success(new UserProfileDto(
                user.UserId, user.Email, user.FullName, user.Phone,
                user.ProfileImage, user.Role.ToString(), user.IsVerified,
                user.CreatedAt, user.LastLoginAt, user.NationalId));
        }
    }

    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuditService _audit;

        public UpdateProfileCommandHandler(IApplicationDbContext context, IAuditService audit)
        { _context = context; _audit = audit; }

        public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken ct)
        {
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, ct);
            if (user == null) return Result<UserProfileDto>.NotFound("User not found.");

            if (!string.IsNullOrEmpty(request.NationalId) && request.NationalId != user.NationalId)
            {
                var exists = await _context.Users.AnyAsync(u => u.NationalId == request.NationalId && u.UserId != request.UserId, ct);
                if (exists) return Result<UserProfileDto>.Conflict("National ID is already in use.");
            }

            user.FullName = request.FullName.Trim();
            user.Phone = request.Phone?.Trim();
            user.NationalId = request.NationalId?.Trim();
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            await _audit.LogAsync(user.UserId, Domain.Enums.EventType.ProfileUpdate, "User updated profile");

            return Result<UserProfileDto>.Success(new UserProfileDto(
                user.UserId, user.Email, user.FullName, user.Phone,
                user.ProfileImage, user.Role.ToString(), user.IsVerified,
                user.CreatedAt, user.LastLoginAt, user.NationalId));
        }
    }

    public class UploadProfileImageCommandHandler : IRequestHandler<UploadProfileImageCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileStorageService _storage;

        public UploadProfileImageCommandHandler(IApplicationDbContext context, IFileStorageService storage)
        { _context = context; _storage = storage; }

        public async Task<Result<string>> Handle(UploadProfileImageCommand request, CancellationToken ct)
        {
            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!_storage.IsValidFileType(request.ContentType, allowed))
                return Result<string>.Failure("Only JPEG, PNG, and WebP images are allowed.");
            if (!_storage.IsValidFileSize(request.ImageStream.Length, 5 * 1024 * 1024))
                return Result<string>.Failure("Image size must not exceed 5MB.");

            var user = await _context.Users.FindAsync(new object[] { request.UserId }, ct);
            if (user == null) return Result<string>.NotFound("User not found.");

            if (!string.IsNullOrEmpty(user.ProfileImage))
                await _storage.DeleteAsync(user.ProfileImage, ct);

            var filePath = await _storage.UploadAsync(request.ImageStream, request.FileName, request.ContentType, "profile-images", ct);
            user.ProfileImage = filePath;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return Result<string>.Success(_storage.GetPublicUrl(filePath));
        }
    }
}
