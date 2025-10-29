using AutoMapper;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Exceptions;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.DTOs.Common;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace WorkflowManagement.Application.Services
{
    /// <summary>
    /// Service for managing workflow scheduling
    /// </summary>
    public class SchedulingService : ISchedulingService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowScheduleRepository _scheduleRepository;
        private readonly IWorkflowExecutionService _workflowExecutionService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<SchedulingService> _logger;

        public SchedulingService(
            IWorkflowRepository workflowRepository,
            IWorkflowScheduleRepository scheduleRepository,
            IWorkflowExecutionService workflowExecutionService,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<SchedulingService> logger)
        {
            _workflowRepository = workflowRepository;
            _scheduleRepository = scheduleRepository;
            _workflowExecutionService = workflowExecutionService;
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        #region Interface Implementation Methods

        public async Task<ResponseDto<WorkflowScheduleDto>> CreateAsync(WorkflowSchedule schedule, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating schedule for workflow {WorkflowId}", schedule.WorkflowId);

                // Validate workflow exists
                var workflow = await _workflowRepository.GetByIdAsync(schedule.WorkflowId, cancellationToken);
                if (workflow == null)
                {
                    return ResponseDto<WorkflowScheduleDto>.Failure("Workflow not found");
                }

                // Validate cron expression
                if (!IsValidCronExpression(schedule.CronExpression))
                {
                    return ResponseDto<WorkflowScheduleDto>.Failure("Invalid cron expression");
                }

                // Set initial values
                schedule.Id = Guid.NewGuid();
                schedule.CreatedAt = DateTime.UtcNow;
                schedule.NextExecutionAt = CalculateNextRunTime(schedule.CronExpression, schedule.TimeZone?.ToString() ?? "UTC");

                var createdSchedule = await _scheduleRepository.AddAsync(schedule, cancellationToken);
                var scheduleDto = _mapper.Map<WorkflowScheduleDto>(createdSchedule);

                _logger.LogInformation("Schedule created successfully for workflow {WorkflowId}", schedule.WorkflowId);
                return ResponseDto<WorkflowScheduleDto>.CreateSuccess(scheduleDto, "Schedule created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule for workflow {WorkflowId}", schedule.WorkflowId);
                return ResponseDto<WorkflowScheduleDto>.Failure("Failed to create schedule", ex.Message);
            }
        }

        public async Task<ResponseDto<WorkflowScheduleDto>> UpdateAsync(WorkflowSchedule schedule, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating schedule {ScheduleId}", schedule.Id);

                var existingSchedule = await _scheduleRepository.GetByIdAsync(schedule.Id, cancellationToken);
                if (existingSchedule == null)
                {
                    return ResponseDto<WorkflowScheduleDto>.Failure("Schedule not found");
                }

                // Validate cron expression
                if (!IsValidCronExpression(schedule.CronExpression))
                {
                    return ResponseDto<WorkflowScheduleDto>.Failure("Invalid cron expression");
                }

                // Update properties
                existingSchedule.Name = schedule.Name;
                existingSchedule.Description = schedule.Description;
                existingSchedule.CronExpression = schedule.CronExpression;
                existingSchedule.IsActive = schedule.IsActive;
                existingSchedule.TimeZone = schedule.TimeZone;
                existingSchedule.StartDate = schedule.StartDate;
                existingSchedule.EndDate = schedule.EndDate;
                existingSchedule.UpdatedAt = DateTime.UtcNow;
                existingSchedule.NextExecutionAt = CalculateNextRunTime(schedule.CronExpression, schedule.TimeZone?.ToString() ?? "UTC", schedule.StartDate);

                var updatedSchedule = await _scheduleRepository.UpdateAsync(existingSchedule, cancellationToken);
                var scheduleDto = _mapper.Map<WorkflowScheduleDto>(updatedSchedule);

                _logger.LogInformation("Schedule updated successfully {ScheduleId}", schedule.Id);
                return ResponseDto<WorkflowScheduleDto>.CreateSuccess(scheduleDto, "Schedule updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule {ScheduleId}", schedule.Id);
                return ResponseDto<WorkflowScheduleDto>.Failure("Failed to update schedule", ex.Message);
            }
        }

        public async Task<ResponseDto<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting schedule {ScheduleId}", id);

                var schedule = await _scheduleRepository.GetByIdAsync(id, cancellationToken);
                if (schedule == null)
                {
                    return ResponseDto<bool>.Failure("Schedule not found");
                }

                await _scheduleRepository.DeleteAsync(id, cancellationToken);

                _logger.LogInformation("Schedule deleted successfully {ScheduleId}", id);
                return ResponseDto<bool>.CreateSuccess(true, "Schedule deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule {ScheduleId}", id);
                return ResponseDto<bool>.Failure("Failed to delete schedule", ex.Message);
            }
        }

        public async Task<ResponseDto<WorkflowScheduleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var schedule = await _scheduleRepository.GetByIdAsync(id, cancellationToken);
                if (schedule == null)
                {
                    return ResponseDto<WorkflowScheduleDto>.Failure("Schedule not found");
                }

                var scheduleDto = _mapper.Map<WorkflowScheduleDto>(schedule);
                return ResponseDto<WorkflowScheduleDto>.CreateSuccess(scheduleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule {ScheduleId}", id);
                return ResponseDto<WorkflowScheduleDto>.Failure("Failed to get schedule", ex.Message);
            }
        }

        public async Task<ResponseDto<IEnumerable<WorkflowScheduleDto>>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            try
            {
                var schedules = await _scheduleRepository.GetByWorkflowIdAsync(workflowId, cancellationToken);
                var scheduleDtos = schedules.Select(s => _mapper.Map<WorkflowScheduleDto>(s));

                return ResponseDto<IEnumerable<WorkflowScheduleDto>>.CreateSuccess(scheduleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedules for workflow {WorkflowId}", workflowId);
                return ResponseDto<IEnumerable<WorkflowScheduleDto>>.Failure("Failed to get schedules", ex.Message);
            }
        }

        public async Task<ResponseDto<IEnumerable<WorkflowScheduleDto>>> GetActiveSchedulesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting all active schedules");

                var activeSchedules = await _scheduleRepository.GetActiveSchedulesAsync(cancellationToken);
                var scheduleDtos = activeSchedules.Select(s => _mapper.Map<WorkflowScheduleDto>(s));

                return ResponseDto<IEnumerable<WorkflowScheduleDto>>.CreateSuccess(scheduleDtos, "Active schedules retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active schedules");
                return ResponseDto<IEnumerable<WorkflowScheduleDto>>.Failure("Failed to get active schedules", ex.Message);
            }
        }

        public async Task<ResponseDto<IEnumerable<WorkflowScheduleDto>>> GetDueSchedulesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting due schedules");

                var dueSchedules = await _scheduleRepository.GetDueSchedulesAsync(cancellationToken);
                var scheduleDtos = dueSchedules.Select(s => _mapper.Map<WorkflowScheduleDto>(s));

                return ResponseDto<IEnumerable<WorkflowScheduleDto>>.CreateSuccess(scheduleDtos, "Due schedules retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting due schedules");
                return ResponseDto<IEnumerable<WorkflowScheduleDto>>.Failure("Failed to get due schedules", ex.Message);
            }
        }

        public async Task<ResponseDto<bool>> EnableScheduleAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Enabling schedule {ScheduleId}", id);

                var schedule = await _scheduleRepository.GetByIdAsync(id, cancellationToken);
                if (schedule == null)
                {
                    return ResponseDto<bool>.Failure("Schedule not found");
                }

                // Validate workflow is published
                var workflow = await _workflowRepository.GetByIdAsync(schedule.WorkflowId, cancellationToken);
                if (workflow != null && workflow.Status != WorkflowStatus.Published)
                {
                    return ResponseDto<bool>.Failure("Only published workflows can have active schedules");
                }

                schedule.IsActive = true;
                schedule.UpdatedAt = DateTime.UtcNow;
                schedule.NextExecutionAt = CalculateNextRunTime(
                    schedule.CronExpression,
                    schedule.TimeZone?.ToString() ?? "UTC",
                    schedule.StartDate);

                await _scheduleRepository.UpdateAsync(schedule, cancellationToken);

                _logger.LogInformation("Schedule enabled successfully {ScheduleId}", id);
                return ResponseDto<bool>.CreateSuccess(true, "Schedule enabled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling schedule {ScheduleId}", id);
                return ResponseDto<bool>.Failure("Failed to enable schedule", ex.Message);
            }
        }

        public async Task<ResponseDto<bool>> DisableScheduleAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Disabling schedule {ScheduleId}", id);

                var schedule = await _scheduleRepository.GetByIdAsync(id, cancellationToken);
                if (schedule == null)
                {
                    return ResponseDto<bool>.Failure("Schedule not found");
                }

                schedule.IsActive = false;
                schedule.UpdatedAt = DateTime.UtcNow;

                await _scheduleRepository.UpdateAsync(schedule, cancellationToken);

                _logger.LogInformation("Schedule disabled successfully {ScheduleId}", id);
                return ResponseDto<bool>.CreateSuccess(true, "Schedule disabled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling schedule {ScheduleId}", id);
                return ResponseDto<bool>.Failure("Failed to disable schedule", ex.Message);
            }
        }

        public async Task<ResponseDto<DateTime?>> GetNextRunTimeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var schedule = await _scheduleRepository.GetByIdAsync(id, cancellationToken);
                if (schedule == null)
                {
                    return ResponseDto<DateTime?>.Failure("Schedule not found");
                }

                var nextRunTime = CalculateNextRunTime(
                    schedule.CronExpression,
                    schedule.TimeZone?.ToString() ?? "UTC",
                    schedule.StartDate);

                return ResponseDto<DateTime?>.CreateSuccess(nextRunTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next run time for schedule {ScheduleId}", id);
                return ResponseDto<DateTime?>.Failure("Failed to get next run time", ex.Message);
            }
        }

        #endregion

        #region Legacy Methods (kept for backward compatibility)

        public async Task<ResponseDto<WorkflowScheduleDto>> CreateScheduleAsync(Guid workflowId, string cronExpression,
            string timeZone = "UTC", DateTime? startDate = null, DateTime? endDate = null, string? description = null)
        {
            var schedule = new WorkflowSchedule
            {
                WorkflowId = workflowId,
                CronExpression = cronExpression,
                TimeZone = timeZone,
                StartDate = startDate,
                EndDate = endDate,
                Description = description,
                IsActive = true,
                Name = $"Schedule for Workflow {workflowId}"
            };

            return await CreateAsync(schedule);
        }

        public async Task<ResponseDto<WorkflowScheduleDto>> UpdateScheduleAsync(Guid workflowId, string cronExpression,
            string? timeZone = null, DateTime? startDate = null, DateTime? endDate = null, string? description = null)
        {
            try
            {
                var schedulesResult = await GetByWorkflowIdAsync(workflowId);
                if (!schedulesResult.Success || !schedulesResult.Data.Any())
                {
                    return ResponseDto<WorkflowScheduleDto>.Failure("No schedule found for workflow");
                }

                var existingScheduleDto = schedulesResult.Data.First(s => s.IsActive);
                var existingSchedule = await _scheduleRepository.GetByIdAsync(existingScheduleDto.Id);

                if (existingSchedule == null)
                {
                    return ResponseDto<WorkflowScheduleDto>.Failure("Schedule not found");
                }

                existingSchedule.CronExpression = cronExpression;
                existingSchedule.TimeZone = timeZone ?? existingSchedule.TimeZone;
                existingSchedule.StartDate = startDate ?? existingSchedule.StartDate;
                existingSchedule.EndDate = endDate ?? existingSchedule.EndDate;
                existingSchedule.Description = description ?? existingSchedule.Description;

                return await UpdateAsync(existingSchedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule for workflow {WorkflowId}", workflowId);
                return ResponseDto<WorkflowScheduleDto>.Failure("Failed to update schedule", ex.Message);
            }
        }

        public async Task<ResponseDto<bool>> EnableScheduleAsync(Guid workflowId)
        {
            try
            {
                var schedulesResult = await GetByWorkflowIdAsync(workflowId);
                if (!schedulesResult.Success || !schedulesResult.Data.Any())
                {
                    return ResponseDto<bool>.Failure("No schedule found for workflow");
                }

                var activeSchedule = schedulesResult.Data.FirstOrDefault();
                if (activeSchedule == null)
                {
                    return ResponseDto<bool>.Failure("No schedule found for workflow");
                }

                return await EnableScheduleAsync(activeSchedule.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling schedule for workflow {WorkflowId}", workflowId);
                return ResponseDto<bool>.Failure("Failed to enable schedule", ex.Message);
            }
        }

        public async Task<ResponseDto<bool>> DeleteScheduleAsync(Guid workflowId)
        {
            try
            {
                var schedulesResult = await GetByWorkflowIdAsync(workflowId);
                if (!schedulesResult.Success || !schedulesResult.Data.Any())
                {
                    return ResponseDto<bool>.Failure("No schedule found for workflow");
                }

                var scheduleToDelete = schedulesResult.Data.First();
                return await DeleteAsync(scheduleToDelete.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule for workflow {WorkflowId}", workflowId);
                return ResponseDto<bool>.Failure("Failed to delete schedule", ex.Message);
            }
        }

        public async Task<ResponseDto<WorkflowScheduleDto>> GetScheduleAsync(Guid workflowId)
        {
            try
            {
                var schedulesResult = await GetByWorkflowIdAsync(workflowId);
                if (!schedulesResult.Success || !schedulesResult.Data.Any())
                {
                    return ResponseDto<WorkflowScheduleDto>.Failure("No schedule found for workflow");
                }

                var activeSchedule = schedulesResult.Data.FirstOrDefault(s => s.IsActive)
                                   ?? schedulesResult.Data.First();

                return ResponseDto<WorkflowScheduleDto>.CreateSuccess(activeSchedule, "Schedule retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule for workflow {WorkflowId}", workflowId);
                return ResponseDto<WorkflowScheduleDto>.Failure("Failed to get schedule", ex.Message);
            }
        }

        public async Task<ResponseDto<List<DateTime>>> GetNextRunTimesAsync(Guid workflowId, int count = 5)
        {
            try
            {
                var schedulesResult = await GetByWorkflowIdAsync(workflowId);
                if (!schedulesResult.Success || !schedulesResult.Data.Any())
                {
                    return ResponseDto<List<DateTime>>.Failure("No schedule found for workflow");
                }

                var activeSchedule = schedulesResult.Data.FirstOrDefault(s => s.IsActive);
                if (activeSchedule == null)
                {
                    return ResponseDto<List<DateTime>>.Failure("No active schedule found for workflow");
                }

                var nextRuns = CalculateNextRunTimes(
                    activeSchedule.CronExpression,
                    activeSchedule.TimeZone,
                    count,
                    activeSchedule.StartDate,
                    activeSchedule.EndDate);

                return ResponseDto<List<DateTime>>.CreateSuccess(nextRuns, "Next run times calculated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next run times for workflow {WorkflowId}", workflowId);
                return ResponseDto<List<DateTime>>.Failure("Failed to calculate next run times", ex.Message);
            }
        }

        public async Task<ResponseDto<bool>> ExecuteScheduledWorkflowsAsync()
        {
            try
            {
                _logger.LogInformation("Executing scheduled workflows");

                var dueSchedulesResult = await GetDueSchedulesAsync();
                if (!dueSchedulesResult.Success || dueSchedulesResult.Data == null)
                {
                    return ResponseDto<bool>.CreateSuccess(true, "No due schedules found");
                }

                var executedCount = 0;
                var failedCount = 0;

                foreach (var schedule in dueSchedulesResult.Data)
                {
                    try
                    {
                        // Get workflow to determine owner
                        var workflow = await _workflowRepository.GetByIdAsync(schedule.WorkflowId);
                        if (workflow == null)
                        {
                            failedCount++;
                            continue;
                        }

                        var execution = await _workflowExecutionService.ExecuteAsync(
                            schedule.WorkflowId,
                            workflow.OwnerId, // Use workflow owner as executor
                            null, // No input data
                            "scheduled");

                        executedCount++;
                        await UpdateNextRunTimeAsync(schedule.WorkflowId);

                        _logger.LogInformation("Successfully executed scheduled workflow {WorkflowId} with execution {ExecutionId}",
                            schedule.WorkflowId, execution.Id);
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        _logger.LogError(ex, "Error executing scheduled workflow {WorkflowId}", schedule.WorkflowId);
                    }
                }

                var message = $"Executed {executedCount} scheduled workflows successfully. {failedCount} failed.";
                _logger.LogInformation(message);

                return ResponseDto<bool>.CreateSuccess(true, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing scheduled workflows");
                return ResponseDto<bool>.Failure("Failed to execute scheduled workflows", ex.Message);
            }
        }

        public async Task<ResponseDto<bool>> ValidateCronExpressionAsync(string cronExpression)
        {
            try
            {
                var isValid = IsValidCronExpression(cronExpression);
                var message = isValid ? "Cron expression is valid" : "Cron expression is invalid";

                return isValid ?
                    ResponseDto<bool>.CreateSuccess(true, message) :
                    ResponseDto<bool>.Failure(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cron expression {CronExpression}", cronExpression);
                return ResponseDto<bool>.Failure("Failed to validate cron expression", ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private bool IsValidCronExpression(string? cronExpression)
        {
            if (string.IsNullOrEmpty(cronExpression))
                return false;

            try
            {
                CrontabSchedule.Parse(cronExpression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidTimeZone(string timeZone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private DateTime? CalculateNextRunTime(string? cronExpression, string timeZone, DateTime? startDate = null)
        {
            if (string.IsNullOrEmpty(cronExpression))
                return null;

            try
            {
                var schedule = CrontabSchedule.Parse(cronExpression);
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

                var baseTime = startDate.HasValue && startDate > currentTime ? startDate.Value : currentTime;
                var nextRun = schedule.GetNextOccurrence(baseTime);

                return TimeZoneInfo.ConvertTimeToUtc(nextRun, timeZoneInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next run time for cron expression {CronExpression}", cronExpression);
                return null;
            }
        }

        private List<DateTime> CalculateNextRunTimes(string cronExpression, string timeZone, int count,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var nextRuns = new List<DateTime>();

            try
            {
                var schedule = CrontabSchedule.Parse(cronExpression);
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

                var baseTime = startDate.HasValue && startDate > currentTime ? startDate.Value : currentTime;
                var nextRun = baseTime;

                for (int i = 0; i < count; i++)
                {
                    nextRun = schedule.GetNextOccurrence(nextRun);

                    if (endDate.HasValue && nextRun > endDate.Value)
                        break;

                    nextRuns.Add(TimeZoneInfo.ConvertTimeToUtc(nextRun, timeZoneInfo));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next run times for cron expression {CronExpression}", cronExpression);
            }

            return nextRuns;
        }

        private async Task UpdateNextRunTimeAsync(Guid workflowId)
        {
            try
            {
                var schedulesResult = await GetByWorkflowIdAsync(workflowId);
                if (schedulesResult.Success && schedulesResult.Data.Any())
                {
                    var activeScheduleDto = schedulesResult.Data.FirstOrDefault(s => s.IsActive);
                    if (activeScheduleDto != null)
                    {
                        var schedule = await _scheduleRepository.GetByIdAsync(activeScheduleDto.Id);
                        if (schedule != null)
                        {
                            schedule.LastExecutedAt = DateTime.UtcNow;
                            schedule.NextExecutionAt = CalculateNextRunTime(
                                schedule.CronExpression,
                                schedule.TimeZone?.ToString() ?? "UTC",
                                schedule.StartDate);

                            await _scheduleRepository.UpdateAsync(schedule);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating next run time for workflow {WorkflowId}", workflowId);
            }
        }

        #endregion
    }

    #region DTOs

    public class WorkflowScheduleDto
    {
        public Guid Id { get; set; }
        public Guid WorkflowId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public string TimeZone { get; set; } = "UTC";
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextExecutionAt { get; set; }
        public DateTime? LastExecutedAt { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    #endregion
}