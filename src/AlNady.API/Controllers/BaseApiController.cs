using AlNady.Shared.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlNady.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub"), out var id) ? id : 0;

    protected string CurrentUserEmail =>
        User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    protected string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    protected string? IpAddress =>
        HttpContext.Connection.RemoteIpAddress?.ToString();

    protected string? UserAgent =>
        Request.Headers.UserAgent.FirstOrDefault();

    protected IActionResult ToActionResult<T>(Result<T> result) => result.StatusCode switch
    {
        200 => Ok(new { success = true, data = result.Data }),
        201 => StatusCode(201, new { success = true, data = result.Data }),
        204 => NoContent(),
        400 => BadRequest(new { success = false, message = result.ErrorMessage, errors = result.Errors }),
        401 => Unauthorized(new { success = false, message = result.ErrorMessage }),
        403 => StatusCode(403, new { success = false, message = result.ErrorMessage }),
        404 => NotFound(new { success = false, message = result.ErrorMessage }),
        409 => Conflict(new { success = false, message = result.ErrorMessage }),
        _   => StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage })
    };

    protected IActionResult ToActionResult(Result result) => result.StatusCode switch
    {
        200 => Ok(new { success = true }),
        201 => StatusCode(201, new { success = true }),
        204 => NoContent(),
        400 => BadRequest(new { success = false, message = result.ErrorMessage }),
        401 => Unauthorized(new { success = false, message = result.ErrorMessage }),
        403 => StatusCode(403, new { success = false, message = result.ErrorMessage }),
        404 => NotFound(new { success = false, message = result.ErrorMessage }),
        _   => StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage })
    };
}
