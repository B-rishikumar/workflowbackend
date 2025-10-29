using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Entities;

public class WorkflowSchedule : BaseEntity
{
    public Guid WorkflowId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public ScheduleFrequency Frequency { get; set; }
    
    public string? CronExpression { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public DateTime? NextRunTime { get; set; }
    
    public DateTime? LastRunTime { get; set; }
    
    public int RunCount { get; set; } = 0;
    
    public int MaxRuns { get; set; } = 0; // 0 = unlimited
    
    public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;
    
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    // Navigation Properties
    public Workflow Workflow { get; set; } = null!;

    public static implicit operator WorkflowSchedule(WorkflowSchedule v)
    {
        throw new NotImplementedException();
    }
}