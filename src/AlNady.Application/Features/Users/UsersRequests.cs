using AlNady.Application.Features.Users.DTOs;
using AlNady.Application.Interfaces;
using AlNady.Shared.Common;
using MediatR;

namespace AlNady.Application.Features.Users.Queries
{
    public record GetCurrentUserQuery(int UserId) : IRequest<Result<UserProfileDto>>;
    public record GetUserByIdQuery(int UserId) : IRequest<Result<UserProfileDto>>;
}

namespace AlNady.Application.Features.Users.Commands
{
    public record UpdateProfileCommand(int UserId, string FullName, string? Phone, string? NationalId) : IRequest<Result<UserProfileDto>>;
    public record UploadProfileImageCommand(int UserId, Stream ImageStream, string FileName, string ContentType) : IRequest<Result<string>>;
}
