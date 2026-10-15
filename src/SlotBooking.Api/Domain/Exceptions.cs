namespace SlotBooking.Api.Domain;

/// <summary>Expected, user-facing failures. The global exception handler maps each one to a ProblemDetails response.</summary>
public abstract class DomainException(string title, string message, int statusCode) : Exception(message)
{
    public string Title { get; } = title;
    public int StatusCode { get; } = statusCode;
}

public sealed class ValidationException(string message) : DomainException("Invalid request", message, StatusCodes.Status400BadRequest);

public sealed class NotFoundException(string message) : DomainException("Not found", message, StatusCodes.Status404NotFound);

/// <summary>Business or concurrency conflict. CurrentVersion lets the client reload and retry.</summary>
public sealed class ConflictException(string message, uint? currentVersion = null)
    : DomainException("Conflict", message, StatusCodes.Status409Conflict)
{
    public uint? CurrentVersion { get; } = currentVersion;
}
