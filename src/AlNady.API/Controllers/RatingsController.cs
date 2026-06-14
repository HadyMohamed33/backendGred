using AlNady.Application.Features.Ratings.Commands;
using AlNady.Application.Features.Ratings.DTOs;
using AlNady.Application.Features.Ratings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlNady.API.Controllers;

[Route("api")]
public class RatingsController : BaseApiController
{
    private readonly IMediator _mediator;
    public RatingsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get ratings for a program.</summary>
    [HttpGet("programs/{programId:int}/ratings")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetProgramRatings(
        int programId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProgramRatingsQuery(programId, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Submit a rating for a completed program.</summary>
    [HttpPost("programs/{programId:int}/ratings")]
    [Authorize(Roles = "Player")]
    [ProducesResponseType(typeof(RatingDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> SubmitRating(int programId, [FromBody] SubmitRatingRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SubmitRatingCommand(programId, CurrentUserId, request.RatingValue, request.Comment, request.Aspects), ct);
        return ToActionResult(result);
    }

    /// <summary>Edit a rating (within 24 hours of submission).</summary>
    [HttpPut("ratings/{ratingId:int}")]
    [Authorize(Roles = "Player")]
    [ProducesResponseType(typeof(RatingDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> EditRating(int ratingId, [FromBody] SubmitRatingRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new EditRatingCommand(ratingId, CurrentUserId, request.RatingValue, request.Comment, request.Aspects), ct);
        return ToActionResult(result);
    }
}
