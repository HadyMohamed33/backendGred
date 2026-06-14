using AlNady.Application.Features.Forms.Commands;
using AlNady.Application.Features.Forms.DTOs;
using AlNady.Application.Features.Forms.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlNady.API.Controllers;

[Route("api")]
[Authorize]
public class FormsController : BaseApiController
{
    private readonly IMediator _mediator;
    public FormsController(IMediator mediator) => _mediator = mediator;

    // ─── Form Management ─────────────────────────────────────────────────────

    /// <summary>Create enrollment form for a program.</summary>
    [HttpPost("programs/{programId:int}/form")]
    [Authorize(Policy = "RequireTrainerOrAcademy")]
    [ProducesResponseType(typeof(FormDto), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateForm(int programId, [FromBody] CreateFormRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateFormCommand(programId, CurrentUserId, request.Title, request.Fields), ct);
        return ToActionResult(result);
    }

    /// <summary>Get the enrollment form for a program.</summary>
    [HttpGet("programs/{programId:int}/form")]
    [ProducesResponseType(typeof(FormDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetForm(int programId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFormByProgramQuery(programId), ct);
        return ToActionResult(result);
    }

    // ─── Enrollment (Form Response) ──────────────────────────────────────────

    /// <summary>Submit enrollment form response for a program.</summary>
    [HttpPost("forms/{formId:int}/enroll")]
    [Authorize(Roles = "Player")]
    [ProducesResponseType(typeof(FormResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> SubmitEnrollment(int formId, [FromBody] SubmitFormResponseRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SubmitFormResponseCommand(formId, CurrentUserId, request.FieldValues), ct);
        return ToActionResult(result);
    }

    /// <summary>Get my enrollments.</summary>
    [HttpGet("enrollments/mine")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetMyEnrollments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMyEnrollmentsQuery(CurrentUserId, page, pageSize), ct);
        return ToActionResult(result);
    }

    /// <summary>Get enrollments for a specific program (trainer/academy/admin).</summary>
    [HttpGet("programs/{programId:int}/enrollments")]
    [Authorize(Policy = "RequireTrainerOrAcademy")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetProgramEnrollments(
        int programId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetProgramEnrollmentsQuery(programId, CurrentUserId, page, pageSize), ct);
        return ToActionResult(result);
    }
}
