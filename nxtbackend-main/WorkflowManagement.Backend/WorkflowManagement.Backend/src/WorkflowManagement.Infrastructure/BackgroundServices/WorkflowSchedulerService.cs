// WorkflowSchedulerService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using WorkflowManagement.Core.Interfaces.Services;

namespace WorkflowManagement.Infrastructure.BackgroundServices;

[DisallowConcurrentExecution]
public class WorkflowSchedulerService : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowSchedulerService> _logger;

    public WorkflowSchedulerService(IServiceProvider serviceProvider, ILogger<WorkflowSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Workflow scheduler job started");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var schedulingService = scope.ServiceProvider.GetRequiredService<ISchedulingService>();
            var workflowExecutionService = scope.ServiceProvider.GetRequiredService<IWorkflowExecutionService>();

            // Get due schedules
            var dueSchedules = await schedulingService.GetDueSchedulesAsync(context.CancellationToken);

            foreach (var schedule in dueSchedules)
            {
                try
                {
                    _logger.LogInformation("Executing scheduled workflow {WorkflowId} from schedule {ScheduleId}", 
                        schedule.WorkflowId, schedule.Id);

                    // Execute workflow
                    await workflowExecutionService.ExecuteAsync(
                        schedule.WorkflowId,
                        Guid.Empty, // System user
                        schedule.Parameters,
                        "scheduled",
                        context.CancellationToken);

                    // Update schedule run count and next run time
                    // This would be handled by the scheduling service
                    _logger.LogInformation("Successfully executed scheduled workflow {WorkflowId}", schedule.WorkflowId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing scheduled workflow {WorkflowId}", schedule.WorkflowId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in workflow scheduler job");
        }

        _logger.LogInformation("Workflow scheduler job completed");
    }
}