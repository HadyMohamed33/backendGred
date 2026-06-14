namespace AlNady.Shared.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string name, object key) : base($"{name} with key '{key}' was not found.") { }
}

public class ValidationException : Exception
{
    public List<string> Errors { get; }
    public ValidationException(List<string> errors) : base("Validation failed.") => Errors = errors;
    public ValidationException(string error) : base(error) => Errors = new List<string> { error };
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Unauthorized access.") : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Access forbidden.") : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

public class PaymentException : Exception
{
    public string? ProviderErrorCode { get; }
    public PaymentException(string message, string? providerErrorCode = null) : base(message)
        => ProviderErrorCode = providerErrorCode;
}
