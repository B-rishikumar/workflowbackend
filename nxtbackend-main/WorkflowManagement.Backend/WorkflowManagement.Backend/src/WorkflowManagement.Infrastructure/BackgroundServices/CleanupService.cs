// CleanupService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using WorkflowManagement.Infrastructure.Data.Context;

namespace WorkflowManagement.Infrastructure.BackgroundServices;

[DisallowConcurrentExecution]
public class CleanupService : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CleanupService> _logger;

    public CleanupService(
        IServiceProvider serviceProvider, 
        IConfiguration configuration, 
        ILogger<CleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Cleanup job started");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();

            var logRetentionDays = _configuration.GetValue<int>("BackgroundJobs:LogRetentionDays", 30);
            var executionRetentionDays = _configuration.GetValue<int>("BackgroundJobs:ExecutionRetentionDays", 90);

            var logCutoffDate = DateTime.UtcNow.AddDays(-logRetentionDays);
            var executionCutoffDate = DateTime.UtcNow.AddDays(-executionRetentionDays);

            // Clean up old execution logs
            _logger.LogInformation("Cleaning up execution logs older than {CutoffDate}", logCutoffDate);
            var oldLogs = await dbContext.ExecutionLogs
                .Where(l => l.CreatedAt < logCutoffDate)
                .CountAsync(context.CancellationToken);

            if (oldLogs > 0)
            {
                await dbContext.ExecutionLogs
                    .Where(l => l.CreatedAt < logCutoffDate)
                    .ExecuteDeleteAsync(context.CancellationToken);

                _logger.LogInformation("Deleted {Count} old execution logs", oldLogs);
            }

            // Clean up old workflow executions (keep only failed and recent completed ones)
            _logger.LogInformation("Cleaning up workflow executions older than {CutoffDate}", executionCutoffDate);
            var oldExecutions = await dbContext.WorkflowExecutions
                .Where(e => e.CreatedAt < executionCutoffDate && 
                           e.Status == Core.Enums.ExecutionStatus.Completed)
                .CountAsync(context.CancellationToken);

            if (oldExecutions > 0)
            {
                await dbContext.WorkflowExecutions
                    .Where(e => e.CreatedAt < executionCutoffDate && 
                               e.Status == Core.Enums.ExecutionStatus.Completed)
                    .ExecuteDeleteAsync(context.CancellationToken);

                _logger.LogInformation("Deleted {Count} old workflow executions", oldExecutions);
            }

            // Clean up soft-deleted entities (permanent deletion after 30 days)
            var softDeleteCutoff = DateTime.UtcNow.AddDays(-30);

            _logger.LogInformation("Permanently deleting soft-deleted entities older than {CutoffDate}", softDeleteCutoff);

            // Clean up soft-deleted workflows
            var softDeletedWorkflows = await dbContext.Workflows
                .IgnoreQueryFilters()
                .Where(w => w.IsDeleted && w.DeletedAt < softDeleteCutoff)
                .CountAsync(context.CancellationToken);

            if (softDeletedWorkflows > 0)
            {
                await dbContext.Workflows
                    .IgnoreQueryFilters()
                    .Where(w => w.IsDeleted && w.DeletedAt < softDeleteCutoff)
                    .ExecuteDeleteAsync(context.CancellationToken);

                _logger.LogInformation("Permanently deleted {Count} soft-deleted workflows", softDeletedWorkflows);
            }

            // Clean up soft-deleted API endpoints
            var softDeletedEndpoints = await dbContext.ApiEndpoints
                .IgnoreQueryFilters()
                .Where(a => a.IsDeleted && a.DeletedAt < softDeleteCutoff)
                .CountAsync(context.CancellationToken);

            if (softDeletedEndpoints > 0)
            {
                await dbContext.ApiEndpoints
                    .IgnoreQueryFilters()
                    .Where(a => a.IsDeleted && a.DeletedAt < softDeleteCutoff)
                    .ExecuteDeleteAsync(context.CancellationToken);

                _logger.LogInformation("Permanently deleted {Count} soft-deleted API endpoints", softDeletedEndpoints);
            }

            // Clean up expired refresh tokens
            _logger.LogInformation("Cleaning up expired refresh tokens");
            var expiredTokenCount = await dbContext.Users
                .Where(u => u.RefreshTokenExpiryTime < DateTime.UtcNow)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.RefreshToken, (string?)null)
                    .SetProperty(u => u.RefreshTokenExpiryTime, (DateTime?)null),
                    context.CancellationToken);

            if (expiredTokenCount > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired refresh tokens", expiredTokenCount);
            }

            await dbContext.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in cleanup job");
        }

        _logger.LogInformation("Cleanup job completed");
    }
}
