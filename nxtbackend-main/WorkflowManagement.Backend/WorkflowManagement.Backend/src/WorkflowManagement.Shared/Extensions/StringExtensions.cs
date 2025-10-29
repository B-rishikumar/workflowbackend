// Extensions/StringExtensions.cs
using System.Text;

namespace WorkflowManagement.Shared.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new StringBuilder();

        foreach (var word in words)
        {
            if (word.Length > 0)
            {
                result.Append(char.ToUpper(word[0]));
                if (word.Length > 1)
                    result.Append(word[1..].ToLower());
                result.Append(' ');
            }
        }

        return result.ToString().Trim();
    }

    public static string TruncateWithEllipsis(this string input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length <= maxLength)
            return input ?? string.Empty;

        return input[..(maxLength - 3)] + "...";
    }

    public static string ToSlug(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");
    }

    public static string RemoveSpecialCharacters(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var c in input)
        {
            if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                sb.Append(c);
        }
        return sb.ToString();
    }

    public static string ToBase64(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    public static string FromBase64(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        try
        {
            var bytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}
