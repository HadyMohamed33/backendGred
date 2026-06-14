using AlNady.Application.Features.Users.Commands;
using AlNady.Application.Features.Users.DTOs;
using AlNady.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlNady.API.Controllers;

[Route("api/users")]
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;
    public UsersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get current authenticated user profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Get user profile by ID (Admin only).</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Update current user profile.</summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateProfileCommand(CurrentUserId, request.FullName, request.Phone, request.NationalId), ct);
        return ToActionResult(result);
    }

    /// <summary>Upload profile image.</summary>
    [HttpPost("me/profile-image")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UploadProfileImage(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file uploaded." });

        using var stream = file.OpenReadStream();
        var result = await _mediator.Send(
            new UploadProfileImageCommand(CurrentUserId, stream, file.FileName, file.ContentType), ct);
        return ToActionResult(result);
    }
}
