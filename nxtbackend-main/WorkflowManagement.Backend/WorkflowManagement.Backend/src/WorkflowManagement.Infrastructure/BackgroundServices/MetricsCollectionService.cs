// MetricsCollectionService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using WorkflowManagement.Core.Interfaces.Services;

namespace WorkflowManagement.Infrastructure.BackgroundServices;

[DisallowConcurrentExecution]
public class MetricsCollectionService : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MetricsCollectionService> _logger;

    public MetricsCollectionService(IServiceProvider serviceProvider, ILogger<MetricsCollectionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Metrics collection job started");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var metricsService = scope.ServiceProvider.GetRequiredService<IMetricsService>();
            var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

            // Get all active workflows
            var workflows = await workflowService.GetAllAsync(context.CancellationToken);

            var yesterday = DateTime.UtcNow.Date.AddDays(-1);

            foreach (var workflow in workflows.Where(w => w.IsPublished))
            {
                try
                {
                    _logger.LogDebug("Updating metrics for workflow {WorkflowId}", workflow.Id);

                    await metricsService.UpdateMetricsAsync(workflow.Id, yesterday, context.CancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating metrics for workflow {WorkflowId}", workflow.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in metrics collection job");
        }

        _logger.LogInformation("Metrics collection job completed");
    }
}