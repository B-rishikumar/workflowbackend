
// Extensions/DateTimeExtensions.cs
namespace WorkflowManagement.Shared.Extensions;

public static class DateTimeExtensions
{
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        return timeSpan switch
        {
            { TotalSeconds: <= 60 } => "just now",
            { TotalMinutes: <= 1 } => "about a minute ago",
            { TotalMinutes: < 60 } => $"about {(int)timeSpan.TotalMinutes} minutes ago",
            { TotalHours: <= 1 } => "about an hour ago",
            { TotalHours: < 24 } => $"about {(int)timeSpan.TotalHours} hours ago",
            { TotalDays: <= 1 } => "yesterday",
            { TotalDays: < 30 } => $"about {(int)timeSpan.TotalDays} days ago",
            { TotalDays: < 365 } => $"about {(int)(timeSpan.TotalDays / 30)} months ago",
            _ => $"about {(int)(timeSpan.TotalDays / 365)} years ago"
        };
    }

    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind);
    }

    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999, dateTime.Kind);
    }

    public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        var diff = (7 + (dateTime.DayOfWeek - startOfWeek)) % 7;
        return dateTime.AddDays(-1 * diff).StartOfDay();
    }

    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
    }

    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddDays(-1).EndOfDay();
    }

    public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end)
    {
        return dateTime >= start && dateTime <= end;
    }

    public static string ToIso8601String(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }
}
