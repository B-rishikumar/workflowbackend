// PublishWorkflowHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.Commands.Workflows;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Exceptions;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;

namespace WorkflowManagement.Application.Handlers.Commands;

public class PublishWorkflowHandler : IRequestHandler<PublishWorkflowCommand, ResponseDto<WorkflowDto>>
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IWorkflowService _workflowService;
    private readonly IApprovalService _approvalService;
    private readonly IMapper _mapper;
    private readonly ILogger<PublishWorkflowHandler> _logger;

    public PublishWorkflowHandler(
        IWorkflowRepository workflowRepository,
        IWorkflowService workflowService,
        IApprovalService approvalService,
        IMapper mapper,
        ILogger<PublishWorkflowHandler> logger)
    {
        _workflowRepository = workflowRepository;
        _workflowService = workflowService;
        _approvalService = approvalService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseDto<WorkflowDto>> Handle(PublishWorkflowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Publishing workflow: {WorkflowId} by user: {UserId}", request.WorkflowId, request.PublishedById);

            var workflow = await _workflowRepository.GetWithStepsAsync(request.WorkflowId, cancellationToken);
            if (workflow == null)
            {
                throw new NotFoundException("Workflow", request.WorkflowId);
            }

            if (workflow.IsPublished)
            {
                throw new WorkflowException(request.WorkflowId, workflow.Name, "Workflow is already published", WorkflowErrorType.Validation);
            }

            // Check if approval is required
            if (request.RequireApproval)
            {
                // Create approval request
                await _approvalService.RequestApprovalAsync(
                    request.WorkflowId,
                    request.PublishedById,
                    "Publish",
                    request.PublishNotes,
                    cancellationToken);

                return new ResponseDto<WorkflowDto>
                {
                    Success = true,
                    Message = "Approval request submitted. Workflow will be published upon approval.",
                    Data = _mapper.Map<WorkflowDto>(workflow)
                };
            }

            // Publish immediately
            var publishedWorkflow = await _workflowService.PublishAsync(request.WorkflowId, request.PublishedById, cancellationToken);
            var workflowDto = _mapper.Map<WorkflowDto>(publishedWorkflow);

            _logger.LogInformation("Successfully published workflow: {WorkflowId}", request.WorkflowId);

            return new ResponseDto<WorkflowDto>
            {
                Success = true,
                Message = "Workflow published successfully",
                Data = workflowDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing workflow: {WorkflowId}", request.WorkflowId);
            return new ResponseDto<WorkflowDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }
}
