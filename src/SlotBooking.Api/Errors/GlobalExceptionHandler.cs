using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlotBooking.Api.Domain;

namespace SlotBooking.Api.Errors;

/// <summary>
/// Single place that turns exceptions into RFC 7807 ProblemDetails responses and logs them.
/// Expected outcomes (validation, not found, conflicts) log as warnings without stack traces;
/// anything unexpected logs as an error with the full exception.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail, isExpected) = Map(exception);

        var context = new { Method = httpContext.Request.Method, Path = httpContext.Request.Path.Value, TraceId = httpContext.TraceIdentifier };
        if (isExpected)
        {
            logger.LogWarning("Request {Method} {Path} rejected ({Status}): {Message}", context.Method, context.Path, status, exception.Message);
        }
        else
        {
            logger.LogError(exception, "Unhandled error for {Method} {Path} (trace {TraceId})", context.Method, context.Path, context.TraceId);
        }

        httpContext.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
        };

        if (exception is ConflictException { CurrentVersion: { } currentVersion })
        {
            problem.Extensions["currentVersion"] = currentVersion;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem,
        });
    }

    private static (int Status, string Title, string Detail, bool IsExpected) Map(Exception exception) => exception switch
    {
        DomainException domain => (domain.StatusCode, domain.Title, domain.Message, true),

        // Another request changed the row between our read and our write (xmin check failed).
        DbUpdateConcurrencyException => (
            StatusCodes.Status409Conflict,
            "Conflict",
            "The booking was changed by someone else. Reload it and try again.",
            true),

        // The EXCLUDE constraint rejected an overlapping booking.
        DbUpdateException when PostgresErrors.IsExclusionViolation(exception) => (
            StatusCodes.Status409Conflict,
            "Slot already booked",
            "The room is already booked for part of this time range.",
            true),

        _ => (
            StatusCodes.Status500InternalServerError,
            "Unexpected error",
            "An unexpected error occurred. Please try again later.",
            false),
    };
}
