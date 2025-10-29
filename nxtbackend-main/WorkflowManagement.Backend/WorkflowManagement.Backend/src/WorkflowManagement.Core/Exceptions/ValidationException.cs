// ValidationException.cs
using System.Runtime.Serialization;

namespace WorkflowManagement.Core.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
[Serializable]
public class ValidationException : Exception
{
    public IEnumerable<string> Errors { get; }
    public string? PropertyName { get; }
    public object? AttemptedValue { get; }

    public ValidationException() : base("One or more validation errors occurred.")
    {
        Errors = new List<string>();
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        Errors = new List<string> { message };
    }

    public ValidationException(IEnumerable<string> errors) 
        : base("One or more validation errors occurred.")
    {
        Errors = errors ?? new List<string>();
    }

    public ValidationException(string propertyName, string error)
        : base($"Validation failed for property '{propertyName}': {error}")
    {
        PropertyName = propertyName;
        Errors = new List<string> { error };
    }

    public ValidationException(string propertyName, object attemptedValue, string error)
        : base($"Validation failed for property '{propertyName}' with value '{attemptedValue}': {error}")
    {
        PropertyName = propertyName;
        AttemptedValue = attemptedValue;
        Errors = new List<string> { error };
    }

    public ValidationException(Dictionary<string, IEnumerable<string>> validationErrors)
        : base("One or more validation errors occurred.")
    {
        var allErrors = new List<string>();
        foreach (var kvp in validationErrors)
        {
            foreach (var error in kvp.Value)
            {
                allErrors.Add($"{kvp.Key}: {error}");
            }
        }
        Errors = allErrors;
    }

    protected ValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        Errors = info.GetValue(nameof(Errors), typeof(IEnumerable<string>)) as IEnumerable<string> ?? new List<string>();
        PropertyName = info.GetString(nameof(PropertyName));
        AttemptedValue = info.GetValue(nameof(AttemptedValue), typeof(object));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Errors), Errors);
        info.AddValue(nameof(PropertyName), PropertyName);
        info.AddValue(nameof(AttemptedValue), AttemptedValue);
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        if (Errors.Any())
        {
            var errorsString = string.Join(Environment.NewLine, Errors);
            return $"{baseString}{Environment.NewLine}Validation Errors:{Environment.NewLine}{errorsString}";
        }
        return baseString;
    }
}