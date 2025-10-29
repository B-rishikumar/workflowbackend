// NotFoundException.cs
using System.Runtime.Serialization;

namespace WorkflowManagement.Core.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
[Serializable]
public class NotFoundException : Exception
{
    public string? ResourceName { get; }
    public string? ResourceId { get; }

    public NotFoundException() : base("The requested resource was not found.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public NotFoundException(string resourceName, string resourceId)
        : base($"{resourceName} with identifier '{resourceId}' was not found.")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }

    public NotFoundException(string resourceName, Guid resourceId)
        : base($"{resourceName} with identifier '{resourceId}' was not found.")
    {
        ResourceName = resourceName;
        ResourceId = resourceId.ToString();
    }

    public NotFoundException(string resourceName, string resourceId, string message)
        : base(message)
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }

    protected NotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ResourceName = info.GetString(nameof(ResourceName));
        ResourceId = info.GetString(nameof(ResourceId));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ResourceName), ResourceName);
        info.AddValue(nameof(ResourceId), ResourceId);
    }
}