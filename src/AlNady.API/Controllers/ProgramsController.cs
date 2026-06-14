using AlNady.Application.Features.Programs.Commands;
using AlNady.Application.Features.Programs.DTOs;
using AlNady.Application.Features.Programs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlNady.API.Controllers;

[Route("api/programs")]
public class ProgramsController : BaseApiController
{
    private readonly IMediator _mediator;
    public ProgramsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Search training programs with filters.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Search(
        [FromQuery] string? sport,
        [FromQuery] string? location,
        [FromQuery] string? ageCategory,
        [FromQuery] string? gender,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new SearchProgramsQuery(sport, location, ageCategory, gender,
                minPrice, maxPrice, dateFrom, dateTo, status, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Get program by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProgramDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProgramByIdQuery(id), ct);
        return ToActionResult(result);
    }

    /// <summary>Get my programs (trainer/academy).</summary>
    [HttpGet("mine")]
    [Authorize(Policy = "RequireTrainerOrAcademy")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetMyPrograms(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMyProgramsQuery(CurrentUserId, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Create a new training program.</summary>
    [HttpPost]
    [Authorize(Policy = "RequireTrainerOrAcademy")]
    [ProducesResponseType(typeof(ProgramDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateProgramRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateProgramCommand(
            CurrentUserId, request.Title, request.Description, request.SportType,
            request.ProgramDate, request.ProgramTime, request.Price,
            request.Capacity, request.TrainingLocation, request.AgeCategory, request.GenderPreference), ct);
        return ToActionResult(result);
    }

    /// <summary>Update a training program.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "RequireTrainerOrAcademy")]
    [ProducesResponseType(typeof(ProgramDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProgramCommand request, CancellationToken ct)
    {
        var result = await _mediator.Send(request with { ProgramId = id, UserId = CurrentUserId }, ct);
        return ToActionResult(result);
    }

    /// <summary>Cancel / soft-delete a training program.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "RequireTrainerOrAcademy")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteProgramCommand(id, CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Publish a draft program.</summary>
    [HttpPost("{id:int}/publish")]
    [Authorize(Policy = "RequireTrainerOrAcademy")]
    [ProducesResponseType(typeof(ProgramDto), 200)]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new PublishProgramCommand(id, CurrentUserId), ct);
        return ToActionResult(result);
    }
}
