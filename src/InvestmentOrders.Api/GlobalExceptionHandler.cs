using InvestmentOrders.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentOrders.Api;

/// <summary>Single place that turns exceptions into RFC 7807 ProblemDetails responses and logs them.</summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail, isExpected) = Map(exception);

        if (isExpected)
        {
            logger.LogWarning("Request {Method} {Path} rejected ({Status}): {Message}",
                httpContext.Request.Method, httpContext.Request.Path.Value, status, exception.Message);
        }
        else
        {
            logger.LogError(exception, "Unhandled error for {Method} {Path} (trace {TraceId})",
                httpContext.Request.Method, httpContext.Request.Path.Value, httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
        };

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
        _ => (StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred. Please try again later.", false),
    };
}
