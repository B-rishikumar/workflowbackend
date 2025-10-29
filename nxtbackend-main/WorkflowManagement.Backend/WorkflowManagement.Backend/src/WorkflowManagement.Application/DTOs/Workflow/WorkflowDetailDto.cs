using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Application.DTOs.Workflow
{
    /// <summary>
    /// Detailed workflow DTO with complete information
    /// </summary>
    public class WorkflowDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public WorkflowStatus Status { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int EnvironmentId { get; set; }
        public string EnvironmentName { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int Version { get; set; }
        public bool RequiresApproval { get; set; }
        public string? Tags { get; set; }
        public string? Configuration { get; set; }

        // Steps and execution details
        public List<WorkflowStepDetailDto> Steps { get; set; } = new();
        public List<WorkflowVersionDto> Versions { get; set; } = new();
        
        // Schedule information
        public WorkflowScheduleDto? Schedule { get; set; }
        
        // Approval information
        public List<WorkflowApprovalDto> PendingApprovals { get; set; } = new();
        public List<WorkflowApprovalDto> ApprovalHistory { get; set; } = new();

        // Execution statistics
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public DateTime? LastExecutedAt { get; set; }
        public ExecutionStatus? LastExecutionStatus { get; set; }
        public TimeSpan? AverageExecutionTime { get; set; }
        public double SuccessRate { get; set; }

        // Dependencies
        public List<WorkflowDependencyDto> Dependencies { get; set; } = new();
        public List<WorkflowDependencyDto> Dependents { get; set; } = new();
    }

    /// <summary>
    /// Detailed workflow step DTO
    /// </summary>
    public class WorkflowStepDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public int ApiEndpointId { get; set; }
        public string ApiEndpointName { get; set; } = string.Empty;
        public string ApiEndpointUrl { get; set; } = string.Empty;
        public ApiEndpointType ApiEndpointType { get; set; }
        public string? InputMapping { get; set; }
        public string? OutputMapping { get; set; }
        public string? ErrorHandling { get; set; }
        public bool IsOptional { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryCount { get; set; } = 0;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
        public string? Condition { get; set; }
    }

    /// <summary>
    /// Workflow schedule DTO
    /// </summary>
    public class WorkflowScheduleDto
    {
        public int Id { get; set; }
        public string CronExpression { get; set; } = string.Empty;
        public string TimeZone { get; set; } = "UTC";
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextRun { get; set; }
        public DateTime? LastRun { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Workflow approval DTO
    /// </summary>
    public class WorkflowApprovalDto
    {
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public int RequestedByUserId { get; set; }
        public string RequestedByUserName { get; set; } = string.Empty;
        public int? ApproverUserId { get; set; }
        public string? ApproverUserName { get; set; }
        public ApprovalStatus Status { get; set; }
        public string? RequestReason { get; set; }
        public string? ApprovalComments { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string ApprovalType { get; set; } = string.Empty; // Execution, Publish, Update, etc.
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// Workflow dependency DTO
    /// </summary>
    public class WorkflowDependencyDto
    {
        public int Id { get; set; }
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string DependencyType { get; set; } = string.Empty; // Prerequisite, Trigger, etc.
        public string? Condition { get; set; }
        public bool IsRequired { get; set; }
    }

    /// <summary>
    /// Workflow executability check result DTO
    /// </summary>
    public class WorkflowExecutabilityDto
    {
        public int WorkflowId { get; set; }
        public bool CanExecute { get; set; }
        public List<string> BlockingReasons { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public bool RequiresApproval { get; set; }
        public List<WorkflowApprovalDto> PendingApprovals { get; set; } = new();
        public bool HasValidSteps { get; set; }
        public bool AllEndpointsAccessible { get; set; }
        public bool HasValidConfiguration { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Workflows summary statistics DTO
    /// </summary>
    public class WorkflowsSummaryDto
    {
        public int TotalWorkflows { get; set; }
        public int ActiveWorkflows { get; set; }
        public int InactiveWorkflows { get; set; }
        public int DraftWorkflows { get; set; }
        public int PublishedWorkflows { get; set; }
        public int ScheduledWorkflows { get; set; }
        public int WorkflowsRequiringApproval { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int RunningExecutions { get; set; }
        public double OverallSuccessRate { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
        public DateTime? LastExecutionAt { get; set; }
        public List<WorkflowStatusSummaryDto> StatusBreakdown { get; set; } = new();
        public List<ProjectWorkflowSummaryDto> ProjectBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Workflow status summary DTO
    /// </summary>
    public class WorkflowStatusSummaryDto
    {
        public WorkflowStatus Status { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// Project workflow summary DTO
    /// </summary>
    public class ProjectWorkflowSummaryDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int WorkflowCount { get; set; }
        public int ActiveWorkflows { get; set; }
        public int TotalExecutions { get; set; }
        public double SuccessRate { get; set; }
    }
}