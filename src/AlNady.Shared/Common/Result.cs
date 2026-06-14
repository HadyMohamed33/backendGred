namespace AlNady.Shared.Common;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int StatusCode { get; private set; }
    public List<string> Errors { get; private set; } = new();

    private Result() { }

    public static Result<T> Success(T data, int statusCode = 200)
        => new() { IsSuccess = true, Data = data, StatusCode = statusCode };

    public static Result<T> Failure(string error, int statusCode = 400)
        => new() { IsSuccess = false, ErrorMessage = error, StatusCode = statusCode };

    public static Result<T> Failure(List<string> errors, int statusCode = 400)
        => new() { IsSuccess = false, Errors = errors, ErrorMessage = string.Join("; ", errors), StatusCode = statusCode };

    public static Result<T> NotFound(string message = "Resource not found")
        => Failure(message, 404);

    public static Result<T> Unauthorized(string message = "Unauthorized")
        => Failure(message, 401);

    public static Result<T> Forbidden(string message = "Forbidden")
        => Failure(message, 403);

    public static Result<T> Conflict(string message = "Conflict")
        => Failure(message, 409);
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int StatusCode { get; private set; }
    public List<string> Errors { get; private set; } = new();

    private Result() { }

    public static Result Success(int statusCode = 200)
        => new() { IsSuccess = true, StatusCode = statusCode };

    public static Result Failure(string error, int statusCode = 400)
        => new() { IsSuccess = false, ErrorMessage = error, StatusCode = statusCode };

    public static Result NotFound(string message = "Resource not found")
        => Failure(message, 404);

    public static Result Unauthorized(string message = "Unauthorized")
        => Failure(message, 401);

    public static Result Forbidden(string message = "Forbidden")
        => Failure(message, 403);

    public static Result Conflict(string message = "Conflict")
        => Failure(message, 409);

    public static Result Success(string message, int statusCode = 200)
        => new() { IsSuccess = true, StatusCode = statusCode };
}
