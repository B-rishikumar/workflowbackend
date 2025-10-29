// WorkflowErrorType.cs (Enum to support WorkflowException)
namespace WorkflowManagement.Core.Enums;

/// <summary>
/// Types of workflow errors that can occur
/// </summary>
public enum WorkflowErrorType
{
    /// <summary>
    /// General workflow error
    /// </summary>
    General = 0,

    /// <summary>
    /// Workflow validation error
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Workflow execution error
    /// </summary>
    Execution = 2,

    /// <summary>
    /// Workflow step execution error
    /// </summary>
    Step = 3,

    /// <summary>
    /// API call error during workflow execution
    /// </summary>
    ApiCall = 4,

    /// <summary>
    /// Workflow timeout error
    /// </summary>
    Timeout = 5,

    /// <summary>
    /// Workflow configuration error
    /// </summary>
    Configuration = 6,

    /// <summary>
    /// External dependency error
    /// </summary>
    Dependency = 7,

    /// <summary>
    /// Permission or authorization error
    /// </summary>
    Authorization = 8,

    /// <summary>
    /// Resource not found error
    /// </summary>
    NotFound = 9,

    /// <summary>
    /// Concurrency or conflict error
    /// </summary>
    Conflict = 10,

    /// <summary>
    /// Data mapping or transformation error
    /// </summary>
    DataMapping = 11,

    /// <summary>
    /// External service integration error
    /// </summary>
    Integration = 12
}