
// RequestLoggingMiddleware.cs
using Serilog.Context;
using System.Diagnostics;

namespace WorkflowManagement.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("UserId", context.User?.Identity?.Name))
        {
            try
            {
                _logger.LogInformation("Starting request {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await _next(context);

                stopwatch.Stop();
                _logger.LogInformation("Completed request {Method} {Path} with status {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Request {Method} {Path} failed after {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}