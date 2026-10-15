using System.Diagnostics;

namespace InvestmentOrders.Api;

/// <summary>
/// Links every request in one client-side flow (e.g. review -> confirm) under a shared id, independent of
/// each request's own OpenTelemetry trace id (which resets per request and can't do this on its own).
/// The client supplies <see cref="HeaderName"/>; this reflects it onto the current span (filterable in the
/// Aspire dashboard's Traces tab), into the logging scope (filterable in Structured Logs), and echoes it back
/// in the response so the client can log/display it.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var header) && !string.IsNullOrWhiteSpace(header)
            ? header.ToString()
            : Guid.NewGuid().ToString();

        context.Response.Headers[HeaderName] = correlationId;

        Activity.Current?.SetTag("correlation.id", correlationId);
        Activity.Current?.SetBaggage("correlation.id", correlationId);

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }
}
