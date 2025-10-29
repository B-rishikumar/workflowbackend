
// Helpers/ValidationHelper.cs
using System.Text.RegularExpressions;

namespace WorkflowManagement.Shared.Helpers;

public static class ValidationHelper
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        RegexOptions.Compiled);

    private static readonly Regex UsernameRegex = new(
        @"^[a-zA-Z0-9_.-]{3,50}$",
        RegexOptions.Compiled);

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex.IsMatch(email);
    }

    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return password.Length >= 8 && PasswordRegex.IsMatch(password);
    }

    public static bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        return UsernameRegex.IsMatch(username);
    }

    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    public static bool IsValidGuid(string guid)
    {
        return Guid.TryParse(guid, out _);
    }

    public static bool IsValidCronExpression(string cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
            return false;

        // Basic cron validation - you might want to use a dedicated library like Quartz.NET
        var parts = cronExpression.Split(' ');
        return parts.Length == 6; // seconds, minutes, hours, day of month, month, day of week
    }
}