using AutoMapper;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Exceptions;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.DTOs.Common;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WorkflowManagement.Application.Services
{
    /// <summary>
    /// Service for executing workflows and managing workflow executions
    /// </summary>
    public class WorkflowExecutionService : IWorkflowExecutionService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowExecutionRepository _executionRepository;
        private readonly IApiEndpointRepository _apiEndpointRepository;
        private readonly IRequestHandler _requestHandler;
        private readonly IApprovalService _approvalService;
        private readonly INotificationService _notificationService;
        private readonly IMetricsService _metricsService;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkflowExecutionService> _logger;

        public WorkflowExecutionService(
            IWorkflowRepository workflowRepository,
            IWorkflowExecutionRepository executionRepository,
            IApiEndpointRepository apiEndpointRepository,
            IRequestHandler requestHandler,
            IApprovalService approvalService,
            INotificationService notificationService,
            IMetricsService metricsService,
            IMapper mapper,
            ILogger<WorkflowExecutionService> logger)
        {
            _workflowRepository = workflowRepository;
            _executionRepository = executionRepository;
            _apiEndpointRepository = apiEndpointRepository;
            _requestHandler = requestHandler;
            _approvalService = approvalService;
            _notificationService = notificationService;
            _metricsService = metricsService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDto<WorkflowExecutionDto>> ExecuteWorkflowAsync(int workflowId, 
            Dictionary<string, object>? initialInputs = null, string? executionContext = null)
        {
            var executionId = Guid.NewGuid().ToString();
            
            try
            {
                _logger.LogInformation("Starting execution of workflow {WorkflowId} with execution ID {ExecutionId}", 
                    workflowId, executionId);

                // Get workflow with steps
                var workflow = await _workflowRepository.GetWorkflowWithStepsAsync(workflowId);
                if (workflow == null)
                {
                    return ResponseDto<WorkflowExecutionDto>.Failure("Workflow not found");
                }

                // Validate workflow can be executed
                var canExecute = await CanExecuteWorkflowAsync(workflow);
                if (!canExecute.Success)
                {
                    return ResponseDto<WorkflowExecutionDto>.Failure(canExecute.Message);
                }

                // Create execution record
                var execution = new WorkflowExecution
                {
                    Id = executionId,
                    WorkflowId = workflowId,
                    Status = ExecutionStatus.Running,
                    ExecutedAt = DateTime.UtcNow,
                    InputParameters = JsonSerializer.Serialize(initialInputs ?? new Dictionary<string, object>()),
                    ExecutionContext = executionContext,
                    Workflow = workflow
                };

                await _executionRepository.AddAsync(execution);

                // Execute workflow steps
                var result = await ExecuteWorkflowStepsAsync(execution, initialInputs ?? new Dictionary<string, object>());

                // Update execution status
                execution.Status = result.Success ? ExecutionStatus.Completed : ExecutionStatus.Failed;
                execution.CompletedAt = DateTime.UtcNow;
                execution.OutputParameters = JsonSerializer.Serialize(result.Data?.OutputParameters ?? new Dictionary<string, object>());
                execution.ErrorMessage = result.Success ? null : result.Message;

                await _executionRepository.UpdateAsync(execution);

                // Record metrics
                await RecordExecutionMetricsAsync(execution);

                // Send notifications
                await SendExecutionNotificationAsync(execution);

                var executionDto = _mapper.Map<WorkflowExecutionDto>(execution);
                
                _logger.LogInformation("Workflow execution {ExecutionId} completed with status {Status}", 
                    executionId, execution.Status);

                return result.Success 
                    ? ResponseDto<WorkflowExecutionDto>.Success(executionDto, "Workflow executed successfully")
                    : ResponseDto<WorkflowExecutionDto>.Failure(result.Message ?? "Workflow execution failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing workflow {WorkflowId} with execution ID {ExecutionId}", 
                    workflowId, executionId);

                // Update execution record with error
                try
                {
                    var execution = await _executionRepository.GetByIdAsync(executionId);
                    if (execution != null)
                    {
                        execution.Status = ExecutionStatus.Failed;
                        execution.CompletedAt = DateTime.UtcNow;
                        execution.ErrorMessage = ex.Message;
                        await _executionRepository.UpdateAsync(execution);
                    }
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Failed to update execution record after error");
                }

                return ResponseDto<WorkflowExecutionDto>.Failure("An error occurred during workflow execution");
            }
        }

        public async Task<ResponseDto<WorkflowExecutionDto>> GetExecutionAsync(string executionId)
        {
            try
            {
                var execution = await _executionRepository.GetByIdAsync(executionId);
                if (execution == null)
                {
                    return ResponseDto<WorkflowExecutionDto>.Failure("Execution not found");
                }

                var executionDto = _mapper.Map<WorkflowExecutionDto>(execution);
                return ResponseDto<WorkflowExecutionDto>.Success(executionDto, "Execution retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution {ExecutionId}", executionId);
                return ResponseDto<WorkflowExecutionDto>.Failure("An error occurred while retrieving the execution");
            }
        }

        public async Task<ResponseDto<PagedResultDto<WorkflowExecutionDto>>> GetExecutionsAsync(
            int? workflowId = null, ExecutionStatus? status = null, DateTime? startDate = null, 
            DateTime? endDate = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var executions = await _executionRepository.GetPagedExecutionsAsync(
                    workflowId, status, startDate, endDate, page, pageSize);

                var executionDtos = _mapper.Map<List<WorkflowExecutionDto>>(executions.Items);
                
                var pagedResult = new PagedResultDto<WorkflowExecutionDto>
                {
                    Items = executionDtos,
                    TotalCount = executions.TotalCount,
                    Page = page,
                    PageSize = pageSize
                };

                return ResponseDto<PagedResultDto<WorkflowExecutionDto>>.Success(pagedResult, 
                    "Executions retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executions");
                return ResponseDto<PagedResultDto<WorkflowExecutionDto>>.Failure("An error occurred while retrieving executions");
            }
        }

        public async Task<ResponseDto<bool>> CancelExecutionAsync(string executionId)
        {
            try
            {
                _logger.LogInformation("Cancelling execution {ExecutionId}", executionId);

                var execution = await _executionRepository.GetByIdAsync(executionId);
                if (execution == null)
                {
                    return ResponseDto<bool>.Failure("Execution not found");
                }

                if (execution.Status != ExecutionStatus.Running)
                {
                    return ResponseDto<bool>.Failure("Only running executions can be cancelled");
                }

                execution.Status = ExecutionStatus.Cancelled;
                execution.CompletedAt = DateTime.UtcNow;
                execution.ErrorMessage = "Execution cancelled by user";

                await _executionRepository.UpdateAsync(execution);

                _logger.LogInformation("Execution {ExecutionId} cancelled successfully", executionId);
                return ResponseDto<bool>.Success(true, "Execution cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling execution {ExecutionId}", executionId);
                return ResponseDto<bool>.Failure("An error occurred while cancelling the execution");
            }
        }

        public async Task<ResponseDto<bool>> RetryExecutionAsync(string executionId)
        {
            try
            {
                _logger.LogInformation("Retrying execution {ExecutionId}", executionId);

                var originalExecution = await _executionRepository.GetByIdAsync(executionId);
                if (originalExecution == null)
                {
                    return ResponseDto<bool>.Failure("Original execution not found");
                }

                if (originalExecution.Status == ExecutionStatus.Running)
                {
                    return ResponseDto<bool>.Failure("Cannot retry a running execution");
                }

                // Get original input parameters
                var originalInputs = string.IsNullOrEmpty(originalExecution.InputParameters) 
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(originalExecution.InputParameters) ?? new Dictionary<string, object>();

                // Create new execution
                var retryResult = await ExecuteWorkflowAsync(
                    originalExecution.WorkflowId, 
                    originalInputs, 
                    $"Retry of execution {executionId}");

                if (retryResult.Success)
                {
                    _logger.LogInformation("Execution {ExecutionId} retried successfully with new execution ID {NewExecutionId}", 
                        executionId, retryResult.Data?.Id);
                    return ResponseDto<bool>.Success(true, $"Execution retried successfully. New execution ID: {retryResult.Data?.Id}");
                }

                return ResponseDto<bool>.Failure(retryResult.Message ?? "Retry execution failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying execution {ExecutionId}", executionId);
                return ResponseDto<bool>.Failure("An error occurred while retrying the execution");
            }
        }

        public async Task<ResponseDto<List<ExecutionLogDto>>> GetExecutionLogsAsync(string executionId)
        {
            try
            {
                var execution = await _executionRepository.GetByIdAsync(executionId);
                if (execution == null)
                {
                    return ResponseDto<List<ExecutionLogDto>>.Failure("Execution not found");
                }

                var logs = execution.Logs?.OrderBy(l => l.CreatedAt).ToList() ?? new List<ExecutionLog>();
                var logDtos = _mapper.Map<List<ExecutionLogDto>>(logs);

                return ResponseDto<List<ExecutionLogDto>>.Success(logDtos, "Execution logs retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution logs for {ExecutionId}", executionId);
                return ResponseDto<List<ExecutionLogDto>>.Failure("An error occurred while retrieving execution logs");
            }
        }

        public async Task<ResponseDto<bool>> TestWorkflowAsync(int workflowId, Dictionary<string, object>? testInputs = null)
        {
            try
            {
                _logger.LogInformation("Testing workflow {WorkflowId}", workflowId);

                var workflow = await _workflowRepository.GetWorkflowWithStepsAsync(workflowId);
                if (workflow == null)
                {
                    return ResponseDto<bool>.Failure("Workflow not found");
                }

                // Create a test execution (not saved to database)
                var testExecution = new WorkflowExecution
                {
                    Id = $"test_{Guid.NewGuid()}",
                    WorkflowId = workflowId,
                    Status = ExecutionStatus.Running,
                    ExecutedAt = DateTime.UtcNow,
                    ExecutionContext = "Test Execution",
                    Workflow = workflow
                };

                // Test each step without actual execution
                var testResults = await TestWorkflowStepsAsync(workflow, testInputs ?? new Dictionary<string, object>());

                return testResults.Success 
                    ? ResponseDto<bool>.Success(true, "Workflow test completed successfully")
                    : ResponseDto<bool>.Failure(testResults.Message ?? "Workflow test failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing workflow {WorkflowId}", workflowId);
                return ResponseDto<bool>.Failure("An error occurred while testing the workflow");
            }
        }

        #region Private Methods

        private async Task<ResponseDto<bool>> CanExecuteWorkflowAsync(Workflow workflow)
        {
            // Check workflow status
            if (workflow.Status != WorkflowStatus.Published)
            {
                return ResponseDto<bool>.Failure("Only published workflows can be executed");
            }

            // Check if workflow requires approval
            if (workflow.RequiresApproval)
            {
                var hasApproval = await _approvalService.CanUserApproveWorkflowAsync(workflow.Id, 0, "execution");
                if (!hasApproval.Success || !hasApproval.Data)
                {
                    return ResponseDto<bool>.Failure("Workflow execution requires approval");
                }
            }

            // Check if workflow has steps
            if (workflow.Steps == null || !workflow.Steps.Any())
            {
                return ResponseDto<bool>.Failure("Workflow has no executable steps");
            }

            return ResponseDto<bool>.Success(true, "Workflow can be executed");
        }

        private async Task<ResponseDto<WorkflowExecutionResult>> ExecuteWorkflowStepsAsync(
            WorkflowExecution execution, Dictionary<string, object> initialInputs)
        {
            var stepResults = new List<StepExecutionResult>();
            var currentInputs = new Dictionary<string, object>(initialInputs);
            var executionLogs = new List<ExecutionLog>();

            try
            {
                var steps = execution.Workflow.Steps.OrderBy(s => s.Order).ToList();

                foreach (var step in steps)
                {
                    _logger.LogInformation("Executing step {StepName} (Order: {Order}) for execution {ExecutionId}", 
                        step.Name, step.Order, execution.Id);

                    // Log step start
                    var stepStartLog = new ExecutionLog
                    {
                        ExecutionId = execution.Id,
                        StepId = step.Id,
                        LogLevel = "Info",
                        Message = $"Starting execution of step: {step.Name}",
                        CreatedAt = DateTime.UtcNow
                    };
                    executionLogs.Add(stepStartLog);

                    var stepResult = await ExecuteStepAsync(step, currentInputs, execution.Id);
                    stepResults.Add(stepResult);

                    // Log step completion
                    var stepEndLog = new ExecutionLog
                    {
                        ExecutionId = execution.Id,
                        StepId = step.Id,
                        LogLevel = stepResult.Success ? "Info" : "Error",
                        Message = stepResult.Success 
                            ? $"Step {step.Name} completed successfully" 
                            : $"Step {step.Name} failed: {stepResult.ErrorMessage}",
                        CreatedAt = DateTime.UtcNow,
                        Details = stepResult.Details
                    };
                    executionLogs.Add(stepEndLog);

                    if (!stepResult.Success)
                    {
                        if (step.IsOptional)
                        {
                            _logger.LogWarning("Optional step {StepName} failed, continuing execution", step.Name);
                            continue;
                        }

                        _logger.LogError("Required step {StepName} failed, stopping execution", step.Name);
                        break;
                    }

                    // Update inputs for next step with current step outputs
                    if (stepResult.OutputParameters != null)
                    {
                        foreach (var output in stepResult.OutputParameters)
                        {
                            currentInputs[output.Key] = output.Value;
                        }
                    }
                }

                // Save execution logs
                execution.Logs = executionLogs;

                var allStepsSuccessful = stepResults.All(r => r.Success || r.IsOptional);
                var result = new WorkflowExecutionResult
                {
                    Success = allStepsSuccessful,
                    OutputParameters = currentInputs,
                    StepResults = stepResults,
                    Message = allStepsSuccessful ? "Workflow completed successfully" : "One or more required steps failed"
                };

                return ResponseDto<WorkflowExecutionResult>.Success(result, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing workflow steps for execution {ExecutionId}", execution.Id);

                // Log the error
                var errorLog = new ExecutionLog
                {
                    ExecutionId = execution.Id,
                    LogLevel = "Error",
                    Message = $"Workflow execution failed: {ex.Message}",
                    CreatedAt = DateTime.UtcNow,
                    Details = ex.StackTrace
                };
                executionLogs.Add(errorLog);
                execution.Logs = executionLogs;

                return ResponseDto<WorkflowExecutionResult>.Failure("An error occurred during workflow execution");
            }
        }

        private async Task<StepExecutionResult> ExecuteStepAsync(WorkflowStep step, 
            Dictionary<string, object> inputs, string executionId)
        {
            try
            {
                // Get the API endpoint for this step
                var endpoint = await _apiEndpointRepository.GetByIdAsync(step.ApiEndpointId);
                if (endpoint == null)
                {
                    return new StepExecutionResult
                    {
                        Success = false,
                        ErrorMessage = "API endpoint not found",
                        StepId = step.Id,
                        StepName = step.Name,
                        IsOptional = step.IsOptional
                    };
                }

                // Transform inputs based on step configuration
                var transformedInputs = await TransformStepInputsAsync(step, inputs);

                // Execute the API request
                var requestResult = endpoint.Type == ApiEndpointType.REST
                    ? await _requestHandler.ExecuteRestRequestAsync(endpoint, transformedInputs)
                    : await _requestHandler.ExecuteSoapRequestAsync(endpoint, transformedInputs);

                if (!requestResult.IsSuccess)
                {
                    return new StepExecutionResult
                    {
                        Success = false,
                        ErrorMessage = requestResult.ErrorMessage ?? "API request failed",
                        StepId = step.Id,
                        StepName = step.Name,
                        IsOptional = step.IsOptional,
                        Details = requestResult.Content
                    };
                }

                // Extract outputs based on step configuration
                var outputs = await ExtractStepOutputsAsync(step, requestResult);

                return new StepExecutionResult
                {
                    Success = true,
                    StepId = step.Id,
                    StepName = step.Name,
                    IsOptional = step.IsOptional,
                    OutputParameters = outputs,
                    ExecutionTime = requestResult.ExecutionTime,
                    Details = requestResult.Content
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing step {StepName} (ID: {StepId})", step.Name, step.Id);
                
                return new StepExecutionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    StepId = step.Id,
                    StepName = step.Name,
                    IsOptional = step.IsOptional
                };
            }
        }

        private async Task<Dictionary<string, object>> TransformStepInputsAsync(WorkflowStep step, 
            Dictionary<string, object> inputs)
        {
            var transformedInputs = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(step.InputMapping))
            {
                try
                {
                    var mappingConfig = JsonSerializer.Deserialize<Dictionary<string, string>>(step.InputMapping);
                    if (mappingConfig != null)
                    {
                        foreach (var mapping in mappingConfig)
                        {
                            if (inputs.ContainsKey(mapping.Value))
                            {
                                transformedInputs[mapping.Key] = inputs[mapping.Value];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing input mapping for step {StepId}", step.Id);
                    // Fall back to using inputs as-is
                    transformedInputs = inputs;
                }
            }
            else
            {
                transformedInputs = inputs;
            }

            return transformedInputs;
        }

        private async Task<Dictionary<string, object>> ExtractStepOutputsAsync(WorkflowStep step, 
            RequestExecutionResult requestResult)
        {
            var outputs = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(step.OutputMapping))
            {
                try
                {
                    var mappingConfig = JsonSerializer.Deserialize<Dictionary<string, string>>(step.OutputMapping);
                    if (mappingConfig != null)
                    {
                        // Parse the response content
                        var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestResult.Content ?? "{}");
                        if (responseData != null)
                        {
                            foreach (var mapping in mappingConfig)
                            {
                                if (responseData.ContainsKey(mapping.Value))
                                {
                                    outputs[mapping.Key] = responseData[mapping.Value];
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing output mapping for step {StepId}", step.Id);
                }
            }

            // Always include the raw response
            outputs["_raw_response"] = requestResult.Content ?? "";
            outputs["_status_code"] = requestResult.StatusCode;

            return outputs;
        }

        private async Task<ResponseDto<WorkflowTestResult>> TestWorkflowStepsAsync(Workflow workflow, 
            Dictionary<string, object> testInputs)
        {
            var testResults = new List<StepTestResult>();

            foreach (var step in workflow.Steps.OrderBy(s => s.Order))
            {
                try
                {
                    var endpoint = await _apiEndpointRepository.GetByIdAsync(step.ApiEndpointId);
                    if (endpoint == null)
                    {
                        testResults.Add(new StepTestResult
                        {
                            StepId = step.Id,
                            StepName = step.Name,
                            Success = false,
                            ErrorMessage = "API endpoint not found"
                        });
                        continue;
                    }

                    // Test endpoint connectivity
                    var connectionTest = await _requestHandler.TestConnectionAsync(endpoint);
                    
                    testResults.Add(new StepTestResult
                    {
                        StepId = step.Id,
                        StepName = step.Name,
                        Success = connectionTest.IsSuccessful,
                        ErrorMessage = connectionTest.ErrorMessage,
                        ResponseTime = connectionTest.ResponseTime
                    });
                }
                catch (Exception ex)
                {
                    testResults.Add(new StepTestResult
                    {
                        StepId = step.Id,
                        StepName = step.Name,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            var allTestsPassed = testResults.All(r => r.Success);
            var result = new WorkflowTestResult
            {
                Success = allTestsPassed,
                StepResults = testResults,
                Message = allTestsPassed ? "All tests passed" : "One or more tests failed"
            };

            return allTestsPassed 
                ? ResponseDto<WorkflowTestResult>.Success(result, result.Message)
                : ResponseDto<WorkflowTestResult>.Failure(result.Message);
        }

        private async Task RecordExecutionMetricsAsync(WorkflowExecution execution)
        {
            try
            {
                var executionTime = execution.CompletedAt.HasValue 
                    ? (execution.CompletedAt.Value - execution.ExecutedAt).TotalSeconds
                    : 0;

                await _metricsService.RecordMetricsAsync(execution.WorkflowId, "execution_time", executionTime);
                await _metricsService.RecordMetricsAsync(execution.WorkflowId, "execution_count", 1);
                
                if (execution.Status == ExecutionStatus.Completed)
                {
                    await _metricsService.RecordMetricsAsync(execution.WorkflowId, "successful_execution", 1);
                }
                else if (execution.Status == ExecutionStatus.Failed)
                {
                    await _metricsService.RecordMetricsAsync(execution.WorkflowId, "failed_execution", 1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to record execution metrics for execution {ExecutionId}", execution.Id);
            }
        }

        private async Task SendExecutionNotificationAsync(WorkflowExecution execution)
        {
            try
            {
                if (execution.Status == ExecutionStatus.Failed)
                {
                    // Notify workflow owner and stakeholders about failure
                    await _notificationService.SendWorkflowExecutionFailureNotificationAsync(execution.Id);
                }
                else if (execution.Status == ExecutionStatus.Completed)
                {
                    // Optionally notify about successful completion for important workflows
                    // await _notificationService.SendWorkflowExecutionSuccessNotificationAsync(execution.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send execution notification for execution {ExecutionId}", execution.Id);
            }
        }

        #endregion
    }

    #region Result Classes

    public class WorkflowExecutionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> OutputParameters { get; set; } = new();
        public List<StepExecutionResult> StepResults { get; set; } = new();
    }

    public class StepExecutionResult
    {
        public bool Success { get; set; }
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public bool IsOptional { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object>? OutputParameters { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public string? Details { get; set; }
    }

    public class WorkflowTestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<StepTestResult> StepResults { get; set; } = new();
    }

    public class StepTestResult
    {
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan ResponseTime { get; set; }
    }

    public class ExecutionLogDto
    {
        public int Id { get; set; }
        public string ExecutionId { get; set; } = string.Empty;
        public int? StepId { get; set; }
        public string? StepName { get; set; }
        public string LogLevel { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion
}