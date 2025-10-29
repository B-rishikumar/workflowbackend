// INotificationService.cs
namespace WorkflowManagement.Core.Interfaces.Services;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendEmailAsync(IEnumerable<string> to, string subject, string body, CancellationToken cancellationToken = default);
    Task NotifyWorkflowCompletedAsync(Guid executionId, CancellationToken cancellationToken = default);
    Task NotifyWorkflowFailedAsync(Guid executionId, CancellationToken cancellationToken = default);
    Task NotifyApprovalRequestedAsync(Guid approvalId, CancellationToken cancellationToken = default);
    Task NotifyApprovalProcessedAsync(Guid approvalId, CancellationToken cancellationToken = default);
}