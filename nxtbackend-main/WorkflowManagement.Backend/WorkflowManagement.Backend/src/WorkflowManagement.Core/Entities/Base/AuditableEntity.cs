namespace WorkflowManagement.Core.Entities.Base;

public abstract class AuditableEntity : BaseEntity
{
    public string? Version { get; set; }
    
    public string? ChangeReason { get; set; }
    
    public Dictionary<string, object> AuditData { get; set; } = new();
}