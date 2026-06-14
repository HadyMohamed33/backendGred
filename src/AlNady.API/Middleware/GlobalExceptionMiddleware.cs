using AlNady.Shared.Exceptions;
using System.Net;
using System.Text.Json;

namespace AlNady.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException ex       => (HttpStatusCode.NotFound,       ex.Message, (List<string>?)null),
            ValidationException ex     => (HttpStatusCode.BadRequest,     "Validation failed.", ex.Errors),
            UnauthorizedException ex   => (HttpStatusCode.Unauthorized,   ex.Message, null),
            ForbiddenException ex      => (HttpStatusCode.Forbidden,      ex.Message, null),
            ConflictException ex       => (HttpStatusCode.Conflict,       ex.Message, null),
            BusinessRuleException ex   => (HttpStatusCode.UnprocessableEntity, ex.Message, null),
            PaymentException ex        => (HttpStatusCode.PaymentRequired, ex.Message, null),
            OperationCanceledException => (HttpStatusCode.OK,             "Request cancelled.", null),
            _                          => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            success = false,
            statusCode = (int)statusCode,
            message,
            errors,
            traceId = context.TraceIdentifier,
            detail = _env.IsDevelopment() ? exception.ToString() : null
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
