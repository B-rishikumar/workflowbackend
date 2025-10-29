// ISchedulingService.cs
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;

namespace WorkflowManagement.Core.Interfaces.Services;

public interface ISchedulingService
{
    Task<WorkflowSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowSchedule>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowSchedule>> GetActiveSchedulesAsync(CancellationToken cancellationToken = default);
    Task<WorkflowSchedule> CreateAsync(WorkflowSchedule schedule, CancellationToken cancellationToken = default);
    Task<WorkflowSchedule> UpdateAsync(WorkflowSchedule schedule, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EnableScheduleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DisableScheduleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DateTime?> GetNextRunTimeAsync(Guid scheduleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowSchedule>> GetDueSchedulesAsync(CancellationToken cancellationToken = default);
}
