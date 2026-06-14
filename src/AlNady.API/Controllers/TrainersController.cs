using AlNady.Application.Features.Trainers.Commands;
using AlNady.Application.Features.Trainers.DTOs;
using AlNady.Application.Features.Trainers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlNady.API.Controllers;

[Route("api/trainers")]
public class TrainersController : BaseApiController
{
    private readonly IMediator _mediator;
    public TrainersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Search / list trainers with filters.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetTrainers(
        [FromQuery] string? sport,
        [FromQuery] string? ageCategory,
        [FromQuery] string? gender,
        [FromQuery] bool? verifiedOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetTrainersQuery(page, pageSize, sport, ageCategory, gender, verifiedOnly), ct);
        return ToActionResult(result);
    }

    /// <summary>Get a trainer profile by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TrainerProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTrainerById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTrainerByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Get the current user's trainer profile.</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Trainer")]
    [ProducesResponseType(typeof(TrainerProfileDto), 200)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTrainerByUserIdQuery(CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Create a trainer profile for the current user.</summary>
    [HttpPost]
    [Authorize(Roles = "Trainer")]
    [ProducesResponseType(typeof(TrainerProfileDto), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateTrainerProfileRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateTrainerProfileCommand(CurrentUserId, request.About,
                request.SpecializationSports, request.AgeCategory, request.GenderPreference), ct);
        return ToActionResult(result);
    }

    /// <summary>Update current trainer profile.</summary>
    [HttpPut("me")]
    [Authorize(Roles = "Trainer")]
    [ProducesResponseType(typeof(TrainerProfileDto), 200)]
    public async Task<IActionResult> UpdateProfile([FromBody] CreateTrainerProfileRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateTrainerProfileCommand(CurrentUserId, request.About,
                request.SpecializationSports, request.AgeCategory, request.GenderPreference), ct);
        return ToActionResult(result);
    }

    /// <summary>Upload a certificate for the trainer.</summary>
    [HttpPost("me/certificates")]
    [Authorize(Roles = "Trainer")]
    [ProducesResponseType(typeof(CertificateDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UploadCertificate(
        [FromForm] string certificateName,
        IFormFile file,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file uploaded." });

        using var stream = file.OpenReadStream();
        var result = await _mediator.Send(
            new UploadCertificateCommand(CurrentUserId, stream, file.FileName, file.ContentType, certificateName), ct);
        return ToActionResult(result);
    }
}
