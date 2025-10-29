
// Extensions/HttpContextExtensions.cs
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace WorkflowManagement.Shared.Extensions;

public static class HttpContextExtensions
{
    public static Guid? GetUserId(this HttpContext httpContext)
    {
        var userIdClaim = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public static string? GetUserName(this HttpContext httpContext)
    {
        return httpContext.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    public static string? GetUserEmail(this HttpContext httpContext)
    {
        return httpContext.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public static string? GetUserRole(this HttpContext httpContext)
    {
        return httpContext.User?.FindFirst(ClaimTypes.Role)?.Value;
    }

    public static string GetClientIpAddress(this HttpContext httpContext)
    {
        var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(ipAddress))
            ipAddress = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(ipAddress))
            ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        
        return ipAddress ?? "Unknown";
    }

    public static string GetUserAgent(this HttpContext httpContext)
    {
        return httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
    }

    public static bool IsApiRequest(this HttpContext httpContext)
    {
        return httpContext.Request.Path.StartsWithSegments("/api");
    }

    public static string GetCorrelationId(this HttpContext httpContext)
    {
        return httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
               ?? httpContext.TraceIdentifier;
    }
}