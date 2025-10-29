using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.Commands.Workflows;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Exceptions;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;

namespace WorkflowManagement.Application.Handlers.Commands;

public class ExecuteWorkflowHandler : IRequestHandler<ExecuteWorkflowCommand, ResponseDto<WorkflowExecutionDto>>
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IWorkflowExecutionRepository _executionRepository;
    private readonly IWorkflowExecutionService _executionService;
    private readonly IMapper _mapper;
    private readonly ILogger<ExecuteWorkflowHandler> _logger;

    public ExecuteWorkflowHandler(
        IWorkflowRepository workflowRepository,
        IWorkflowExecutionRepository executionRepository,
        IWorkflowExecutionService executionService,
        IMapper mapper,
        ILogger<ExecuteWorkflowHandler> logger)
    {
        _workflowRepository = workflowRepository;
        _executionRepository = executionRepository;
        _executionService = executionService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseDto<WorkflowExecutionDto>> Handle(ExecuteWorkflowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing workflow: {WorkflowId} by user: {UserId}", request.WorkflowId, request.ExecutedById);

            // Validate workflow exists and is published
            var workflow = await _workflowRepository.GetWithStepsAsync(request.WorkflowId, cancellationToken);
            if (workflow == null)
            {
                throw new NotFoundException("Workflow", request.WorkflowId);
            }

            if (!workflow.IsPublished || workflow.Status != WorkflowStatus.Active)
            {
                throw new WorkflowException(request.WorkflowId, workflow.Name, "Workflow is not published or not active", WorkflowErrorType.Validation);
            }

            // Execute the workflow
            var execution = await _executionService.ExecuteAsync(
                request.WorkflowId,
                request.ExecutedById,
                request.InputData,
                request.TriggerType,
                cancellationToken);

            // If waiting for completion, monitor execution
            if (request.WaitForCompletion)
            {
                execution = await WaitForExecutionCompletion(execution.Id, cancellationToken);
            }

            var executionDto = _mapper.Map<WorkflowExecutionDto>(execution);

            _logger.LogInformation("Successfully started workflow execution with ID: {ExecutionId}", execution.Id);

            return new ResponseDto<WorkflowExecutionDto>
            {
                Success = true,
                Message = request.WaitForCompletion && execution.Status == ExecutionStatus.Completed 
                    ? "Workflow executed successfully" 
                    : "Workflow execution started",
                Data = executionDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow: {WorkflowId}", request.WorkflowId);
            return new ResponseDto<WorkflowExecutionDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }

    private async Task<WorkflowExecution> WaitForExecutionCompletion(Guid executionId, CancellationToken cancellationToken)
    {
        const int maxWaitTime = 300; // 5 minutes
        const int pollInterval = 2; // 2 seconds
        var waited = 0;

        while (waited < maxWaitTime && !cancellationToken.IsCancellationRequested)
        {
            var execution = await _executionRepository.GetByIdAsync(executionId, cancellationToken);
            if (execution == null)
                break;

            if (execution.Status == ExecutionStatus.Completed || 
                execution.Status == ExecutionStatus.Failed || 
                execution.Status == ExecutionStatus.Cancelled)
            {
                return execution;
            }

            await Task.Delay(TimeSpan.FromSeconds(pollInterval), cancellationToken);
            waited += pollInterval;
        }

        // Return current state even if not completed
        return await _executionRepository.GetByIdAsync(executionId, cancellationToken) 
               ?? throw new NotFoundException("WorkflowExecution", executionId);
    }
}