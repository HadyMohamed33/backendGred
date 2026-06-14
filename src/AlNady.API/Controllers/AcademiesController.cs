using AlNady.Application.Features.Academies.Commands;
using AlNady.Application.Features.Academies.DTOs;
using AlNady.Application.Features.Academies.Queries;
using AlNady.Application.Features.Trainers.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlNady.API.Controllers;

[Route("api/academies")]
public class AcademiesController : BaseApiController
{
    private readonly IMediator _mediator;
    public AcademiesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Search / list academies with filters.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAcademies(
        [FromQuery] string? sport,
        [FromQuery] string? location,
        [FromQuery] bool? verifiedOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAcademiesQuery(page, pageSize, sport, location, verifiedOnly), ct);
        return ToActionResult(result);
    }

    /// <summary>Get academy by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AcademyProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAcademyById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAcademyByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Create academy profile for current user.</summary>
    [HttpPost]
    [Authorize(Roles = "Academy")]
    [ProducesResponseType(typeof(AcademyProfileDto), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateAcademyProfileRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateAcademyProfileCommand(CurrentUserId, request.SpecializationSports,
                request.Location, request.AgeCategory, request.GenderPreference), ct);
        return ToActionResult(result);
    }

    /// <summary>Update current academy profile.</summary>
    [HttpPut("me")]
    [Authorize(Roles = "Academy")]
    [ProducesResponseType(typeof(AcademyProfileDto), 200)]
    public async Task<IActionResult> UpdateProfile([FromBody] CreateAcademyProfileRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateAcademyProfileCommand(CurrentUserId, request.SpecializationSports,
                request.Location, request.AgeCategory, request.GenderPreference), ct);
        return ToActionResult(result);
    }

    /// <summary>Upload certificate for the academy.</summary>
    [HttpPost("me/certificates")]
    [Authorize(Roles = "Academy")]
    [ProducesResponseType(201)]
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
            new UploadAcademyCertificateCommand(CurrentUserId, stream, file.FileName, file.ContentType, certificateName), ct);
        return ToActionResult(result);
    }
}
