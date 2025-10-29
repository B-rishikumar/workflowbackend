// RateLimitingMiddleware.cs
using System.Net;

namespace WorkflowManagement.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly Dictionary<string, (DateTime LastRequest, int RequestCount)> _requestCounts = new();
    private readonly int _maxRequests = 100;
    private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);

        if (IsRateLimited(clientId))
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Add("Retry-After", _timeWindow.TotalSeconds.ToString());
            
            _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
            
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Priority: User ID > IP Address
        return context.User?.Identity?.Name 
               ?? context.Connection.RemoteIpAddress?.ToString() 
               ?? "unknown";
    }

    private bool IsRateLimited(string clientId)
    {
        var now = DateTime.UtcNow;

        lock (_requestCounts)
        {
            if (_requestCounts.TryGetValue(clientId, out var requestInfo))
            {
                if (now - requestInfo.LastRequest > _timeWindow)
                {
                    // Reset the count for new time window
                    _requestCounts[clientId] = (now, 1);
                    return false;
                }

                if (requestInfo.RequestCount >= _maxRequests)
                {
                    return true;
                }

                // Increment the count
                _requestCounts[clientId] = (requestInfo.LastRequest, requestInfo.RequestCount + 1);
            }
            else
            {
                // First request from this client
                _requestCounts[clientId] = (now, 1);
            }

            return false;
        }
    }
}