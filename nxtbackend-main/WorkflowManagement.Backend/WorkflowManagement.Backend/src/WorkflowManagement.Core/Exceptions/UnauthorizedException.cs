// UnauthorizedException.cs
using System.Runtime.Serialization;

namespace WorkflowManagement.Core.Exceptions;

/// <summary>
/// Exception thrown when a user is not authorized to perform an action
/// </summary>
[Serializable]
public class UnauthorizedException : Exception
{
    public string? UserId { get; }
    public string? Permission { get; }
    public string? Resource { get; }

    public UnauthorizedException() : base("Access denied. You are not authorized to perform this action.")
    {
    }

    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public UnauthorizedException(string userId, string permission)
        : base($"User '{userId}' is not authorized to perform '{permission}' action.")
    {
        UserId = userId;
        Permission = permission;
    }

    public UnauthorizedException(string userId, string permission, string resource)
        : base($"User '{userId}' is not authorized to perform '{permission}' action on '{resource}'.")
    {
        UserId = userId;
        Permission = permission;
        Resource = resource;
    }

    public UnauthorizedException(Guid userId, string permission)
        : base($"User '{userId}' is not authorized to perform '{permission}' action.")
    {
        UserId = userId.ToString();
        Permission = permission;
    }

    public UnauthorizedException(Guid userId, string permission, string resource)
        : base($"User '{userId}' is not authorized to perform '{permission}' action on '{resource}'.")
    {
        UserId = userId.ToString();
        Permission = permission;
        Resource = resource;
    }

    protected UnauthorizedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        UserId = info.GetString(nameof(UserId));
        Permission = info.GetString(nameof(Permission));
        Resource = info.GetString(nameof(Resource));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(UserId), UserId);
        info.AddValue(nameof(Permission), Permission);
        info.AddValue(nameof(Resource), Resource);
    }
}