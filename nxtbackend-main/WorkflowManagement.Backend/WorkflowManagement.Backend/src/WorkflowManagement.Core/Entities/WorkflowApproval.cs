using System.ComponentModel.DataAnnotations;
using WorkflowManagement.Core.Entities.Base;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Entities;

public class WorkflowApproval : SoftDeleteEntity
{
    public Guid WorkflowId { get; set; }
    
    public Guid RequestedById { get; set; }
    
    public Guid? ApprovedById { get; set; }
    
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    
    [StringLength(1000)]
    public string? RequestReason { get; set; }
    
    [StringLength(1000)]
    public string? ApprovalComment { get; set; }
    
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ApprovedAt { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    public string ApprovalType { get; set; } = "Publish"; // Publish, Delete, Modify
    
    public Dictionary<string, object> Changes { get; set; } = new();
    
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Navigation Properties
    public Workflow Workflow { get; set; } = null!;
    public User RequestedBy { get; set; } = null!;
    public User? ApprovedBy { get; set; }
}