namespace WorkflowManagement.Core.Entities.Base;

public abstract class SoftDeleteEntity : AuditableEntity
{
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; }
    
    public string? DeletedBy { get; set; }
    
    public string? DeleteReason { get; set; }
}