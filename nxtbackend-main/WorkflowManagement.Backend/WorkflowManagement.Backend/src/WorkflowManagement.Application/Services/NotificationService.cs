// NotificationService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;

namespace WorkflowManagement.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _configuration;
    private readonly IWorkflowExecutionRepository _executionRepository;
    private readonly IApprovalRepository _approvalRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IConfiguration configuration,
        IWorkflowExecutionRepository executionRepository,
        IApprovalRepository approvalRepository,
        IUserRepository userRepository,
        ILogger<NotificationService> logger)
    {
        _configuration = configuration;
        _executionRepository = executionRepository;
        _approvalRepository = approvalRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        await SendEmailAsync(new[] { to }, subject, body, cancellationToken);
    }

    public async Task SendEmailAsync(IEnumerable<string> to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var fromEmail = smtpSettings["FromEmail"];
            var fromName = smtpSettings["FromName"];
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            foreach (var email in to)
            {
                message.To.Add(email);
            }

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", to));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}", string.Join(", ", to));
            throw;
        }
    }

    public async Task NotifyWorkflowCompletedAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var execution = await _executionRepository.GetWithLogsAsync(executionId, cancellationToken);
            if (execution == null)
                return;

            var user = await _userRepository.GetByIdAsync(execution.ExecutedById, cancellationToken);
            if (user == null || string.IsNullOrEmpty(user.Email))
                return;

            var subject = $"Workflow Execution Completed: {execution.Workflow.Name}";
            var body = $@"
                <h2>Workflow Execution Completed</h2>
                <p>Your workflow execution has completed successfully.</p>
                <p><strong>Workflow:</strong> {execution.Workflow.Name}</p>
                <p><strong>Status:</strong> {execution.Status}</p>
                <p><strong>Started:</strong> {execution.StartedAt:yyyy-MM-dd HH:mm:ss}</p>
                <p><strong>Completed:</strong> {execution.CompletedAt:yyyy-MM-dd HH:mm:ss}</p>
                <p><strong>Duration:</strong> {execution.Duration}</p>
                <p><strong>Steps Completed:</strong> {execution.CompletedSteps}/{execution.TotalSteps}</p>
            ";

            await SendEmailAsync(user.Email, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send workflow completion notification for execution {ExecutionId}", executionId);
        }
    }

    public async Task NotifyWorkflowFailedAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var execution = await _executionRepository.GetWithLogsAsync(executionId, cancellationToken);
            if (execution == null)
                return;

            var user = await _userRepository.GetByIdAsync(execution.ExecutedById, cancellationToken);
            if (user == null || string.IsNullOrEmpty(user.Email))
                return;

            var subject = $"Workflow Execution Failed: {execution.Workflow.Name}";
            var body = $@"
                <h2>Workflow Execution Failed</h2>
                <p>Your workflow execution has failed.</p>
                <p><strong>Workflow:</strong> {execution.Workflow.Name}</p>
                <p><strong>Status:</strong> {execution.Status}</p>
                <p><strong>Started:</strong> {execution.StartedAt:yyyy-MM-dd HH:mm:ss}</p>
                <p><strong>Failed:</strong> {execution.CompletedAt:yyyy-MM-dd HH:mm:ss}</p>
                <p><strong>Error:</strong> {execution.ErrorMessage}</p>
                <p><strong>Steps Completed:</strong> {execution.CompletedSteps}/{execution.TotalSteps}</p>
            ";

            await SendEmailAsync(user.Email, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send workflow failure notification for execution {ExecutionId}", executionId);
        }
    }

    public async Task NotifyApprovalRequestedAsync(Guid approvalId, CancellationToken cancellationToken = default)
    {
        try
        {
            var approval = await _approvalRepository.GetByIdAsync(approvalId, cancellationToken);
            if (approval == null)
                return;

            // Get all admin users for approval notifications
            var adminUsers = await _userRepository.GetByRoleAsync("Admin", cancellationToken);
            var emailList = adminUsers.Where(u => !string.IsNullOrEmpty(u.Email)).Select(u => u.Email!);

            if (!emailList.Any())
                return;

            var subject = $"Workflow Approval Required: {approval.Workflow.Name}";
            var body = $@"
                <h2>Workflow Approval Required</h2>
                <p>A workflow requires your approval.</p>
                <p><strong>Workflow:</strong> {approval.Workflow.Name}</p>
                <p><strong>Approval Type:</strong> {approval.ApprovalType}</p>
                <p><strong>Requested By:</strong> {approval.RequestedBy.FullName}</p>
                <p><strong>Requested At:</strong> {approval.RequestedAt:yyyy-MM-dd HH:mm:ss}</p>
                <p><strong>Reason:</strong> {approval.RequestReason}</p>
                <p>Please review and approve or reject this request.</p>
            ";

            await SendEmailAsync(emailList, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval request notification for approval {ApprovalId}", approvalId);
        }
    }

    public async Task NotifyApprovalProcessedAsync(Guid approvalId, CancellationToken cancellationToken = default)
    {
        try
        {
            var approval = await _approvalRepository.GetByIdAsync(approvalId, cancellationToken);
            if (approval == null)
                return;

            var requester = await _userRepository.GetByIdAsync(approval.RequestedById, cancellationToken);
            if (requester == null || string.IsNullOrEmpty(requester.Email))
                return;

            var subject = $"Workflow Approval {approval.Status}: {approval.Workflow.Name}";
            var body = $@"
                <h2>Workflow Approval {approval.Status}</h2>
                <p>Your workflow approval request has been processed.</p>
                <p><strong>Workflow:</strong> {approval.Workflow.Name}</p>
                <p><strong>Approval Type:</strong> {approval.ApprovalType}</p>
                <p><strong>Status:</strong> {approval.Status}</p>
                <p><strong>Processed By:</strong> {approval.ApprovedBy?.FullName}</p>
                <p><strong>Processed At:</strong> {approval.ApprovedAt:yyyy-MM-dd HH:mm:ss}</p>
                <p><strong>Comment:</strong> {approval.ApprovalComment}</p>
            ";

            await SendEmailAsync(requester.Email, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval processed notification for approval {ApprovalId}", approvalId);
        }
    }
}