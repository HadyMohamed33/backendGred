using AlNady.Application.Features.Auth.Commands;
using AlNady.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AlNady.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterCommand(
            request.Email, request.Password, request.FullName,
            request.Phone, request.Role, request.NationalId);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Login with email and password.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password, IpAddress, UserAgent);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Refresh access token using refresh token.</summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(TokenResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var command = new RefreshTokenCommand(request.RefreshToken, IpAddress, UserAgent);
        var result = await _mediator.Send(command, ct);
        return ToActionResult(result);
    }

    /// <summary>Logout and invalidate refresh token.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LogoutCommand(request.RefreshToken), ct);
        return ToActionResult(result);
    }

    /// <summary>Verify email address with OTP code.</summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyEmailCommand(request.Email, request.Code), ct);
        return ToActionResult(result);
    }

    /// <summary>Request password reset code via email.</summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand(request.Email), ct);
        return ToActionResult(result);
    }

    /// <summary>Reset password using OTP code.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ResetPasswordCommand(request.Email, request.Code, request.NewPassword), ct);
        return ToActionResult(result);
    }

    /// <summary>Change password for authenticated user.</summary>
    [HttpPut("change-password")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ChangePasswordCommand(CurrentUserId, request.CurrentPassword, request.NewPassword), ct);
        return ToActionResult(result);
    }
}
