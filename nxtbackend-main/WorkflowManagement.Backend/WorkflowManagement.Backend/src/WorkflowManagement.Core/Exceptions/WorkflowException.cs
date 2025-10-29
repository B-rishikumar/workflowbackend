// WorkflowException.cs
using System.Runtime.Serialization;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Exceptions;

/// <summary>
/// Exception thrown when workflow-related operations fail
/// </summary>
[Serializable]
public class WorkflowException : Exception
{
    public Guid? WorkflowId { get; set; }
    public Guid? ExecutionId { get; set; }
    public Guid? StepId { get; set; }
    public string? WorkflowName { get; set; }
    public string? StepName { get; set; }
    public WorkflowErrorType ErrorType { get; set; }
    public Dictionary<string, object> Context { get; set; }

    public WorkflowException() : base("A workflow error occurred.")
    {
        ErrorType = WorkflowErrorType.General;
        Context = new Dictionary<string, object>();
    }

    public WorkflowException(string message) : base(message)
    {
        ErrorType = WorkflowErrorType.General;
        Context = new Dictionary<string, object>();
    }

    public WorkflowException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorType = WorkflowErrorType.General;
        Context = new Dictionary<string, object>();
    }

    public WorkflowException(string message, WorkflowErrorType errorType) : base(message)
    {
        ErrorType = errorType;
        Context = new Dictionary<string, object>();
    }

    public WorkflowException(Guid workflowId, string message) : base(message)
    {
        WorkflowId = workflowId;
        ErrorType = WorkflowErrorType.General;
        Context = new Dictionary<string, object>();
    }

    public WorkflowException(Guid workflowId, string workflowName, string message) : base(message)
    {
        WorkflowId = workflowId;
        WorkflowName = workflowName;
        ErrorType = WorkflowErrorType.General;
        Context = new Dictionary<string, object>();
    }

    public WorkflowException(Guid workflowId, Guid executionId, string message) : base(message)
    {
        WorkflowId = workflowId;
        ExecutionId = executionId;
        ErrorType = WorkflowErrorType.Execution;
        Context = new Dictionary<string, object>();
    }

    public WorkflowException(Guid workflowId, Guid executionId, Guid stepId, string message) : base(message)
    {
        WorkflowId = workflowId;
        ExecutionId = executionId;
        StepId = stepId;
        ErrorType = WorkflowErrorType.Step;
        Context = new Dictionary<string, object>();
    }

    public WorkflowException(Guid workflowId, string workflowName, Guid stepId, string stepName, string message, WorkflowErrorType errorType) 
        : base(message)
    {
        WorkflowId = workflowId;
        WorkflowName = workflowName;
        StepId = stepId;
        StepName = stepName;
        ErrorType = errorType;
        Context = new Dictionary<string, object>();
    }

    public WorkflowException(string message, WorkflowErrorType errorType, Dictionary<string, object> context) 
        : base(message)
    {
        ErrorType = errorType;
        Context = context ?? new Dictionary<string, object>();
    }

    public WorkflowException(Guid workflowId, Guid executionId, string message, WorkflowErrorType errorType, Exception innerException) 
        : base(message, innerException)
    {
        WorkflowId = workflowId;
        ExecutionId = executionId;
        ErrorType = errorType;
        Context = new Dictionary<string, object>();
    }

    protected WorkflowException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        WorkflowId = info.GetValue(nameof(WorkflowId), typeof(Guid?)) as Guid?;
        ExecutionId = info.GetValue(nameof(ExecutionId), typeof(Guid?)) as Guid?;
        StepId = info.GetValue(nameof(StepId), typeof(Guid?)) as Guid?;
        WorkflowName = info.GetString(nameof(WorkflowName));
        StepName = info.GetString(nameof(StepName));
        ErrorType = (WorkflowErrorType)info.GetInt32(nameof(ErrorType));
        Context = info.GetValue(nameof(Context), typeof(Dictionary<string, object>)) as Dictionary<string, object> 
                 ?? new Dictionary<string, object>();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(WorkflowId), WorkflowId);
        info.AddValue(nameof(ExecutionId), ExecutionId);
        info.AddValue(nameof(StepId), StepId);
        info.AddValue(nameof(WorkflowName), WorkflowName);
        info.AddValue(nameof(StepName), StepName);
        info.AddValue(nameof(ErrorType), (int)ErrorType);
        info.AddValue(nameof(Context), Context);
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        var additionalInfo = new List<string>();

        if (WorkflowId.HasValue)
            additionalInfo.Add($"WorkflowId: {WorkflowId}");
        if (!string.IsNullOrEmpty(WorkflowName))
            additionalInfo.Add($"WorkflowName: {WorkflowName}");
        if (ExecutionId.HasValue)
            additionalInfo.Add($"ExecutionId: {ExecutionId}");
        if (StepId.HasValue)
            additionalInfo.Add($"StepId: {StepId}");
        if (!string.IsNullOrEmpty(StepName))
            additionalInfo.Add($"StepName: {StepName}");
        if (ErrorType != WorkflowErrorType.General)
            additionalInfo.Add($"ErrorType: {ErrorType}");

        if (additionalInfo.Any())
        {
            var infoString = string.Join(", ", additionalInfo);
            return $"{baseString}{Environment.NewLine}Additional Info: {infoString}";
        }

        return baseString;
    }

    // Helper methods for creating specific workflow exceptions
    public static WorkflowException ValidationFailed(Guid workflowId, string workflowName, string message)
    {
        return new WorkflowException(workflowId, workflowName, $"Workflow validation failed: {message}")
        {
            ErrorType = WorkflowErrorType.Validation
        };
    }

    public static WorkflowException ExecutionFailed(Guid workflowId, Guid executionId, string message, Exception innerException = null)
    {
        return new WorkflowException(workflowId, executionId, $"Workflow execution failed: {message}", WorkflowErrorType.Execution, innerException);
    }

    public static WorkflowException StepFailed(Guid workflowId, Guid executionId, Guid stepId, string stepName, string message, Exception innerException = null)
    {
        var exception = new WorkflowException(workflowId, string.Empty, stepId, stepName, $"Workflow step failed: {message}", WorkflowErrorType.Step);
      /*  if (innerException != null && exception.InnerException == null)
        {
            // Create new exception with inner exception
            return new WorkflowException($"Workflow step '{stepName}' failed: {message}", WorkflowErrorType.Step, innerException)
            {
                WorkflowId = workflowId,
                ExecutionId = executionId,
                StepId = stepId,
                StepName = stepName
            };
        }*/
        return exception;
    }

    public static WorkflowException TimeoutExceeded(Guid workflowId, Guid executionId, TimeSpan timeout)
    {
        var exception = new WorkflowException(workflowId, executionId, $"Workflow execution timed out after {timeout}");
        return exception;
       // {
         //   ErrorType = WorkflowErrorType.Timeout
        //};
    }

    public static WorkflowException ApiCallFailed(Guid workflowId, Guid executionId, Guid stepId, string apiUrl, string message, Exception innerException = null)
    {
    //public static WorkflowException ApiCallFailed(Guid workflowId, Guid executionId, Guid stepId, string apiUrl, string message, Exception innerException = null)
        var expection = new WorkflowException(workflowId, executionId, stepId, message);
     
      
        return expection;
    }

    public static WorkflowException ConfigurationError(Guid workflowId, string message)
    {
        return new WorkflowException(workflowId, $"Workflow configuration error: {message}")
        {
            ErrorType = WorkflowErrorType.Configuration
        };
    }

    public static WorkflowException DependencyError(Guid workflowId, string dependencyName, string message)
    {
        return new WorkflowException(workflowId, $"Dependency '{dependencyName}' error: {message}")
        {
            ErrorType = WorkflowErrorType.Dependency,
            Context = new Dictionary<string, object> { ["Dependency"] = dependencyName }
        };
    }
}