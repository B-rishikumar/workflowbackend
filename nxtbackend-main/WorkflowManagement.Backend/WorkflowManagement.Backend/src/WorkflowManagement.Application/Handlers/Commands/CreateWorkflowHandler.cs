// CreateWorkflowHandler.cs// CreateWorkflowHandler.cs
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

namespace WorkflowManagement.Application.Handlers.Commands;

public class CreateWorkflowHandler : IRequestHandler<CreateWorkflowCommand, ResponseDto<WorkflowDto>>
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IEnvironmentRepository _environmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateWorkflowHandler> _logger;

    public CreateWorkflowHandler(
        IWorkflowRepository workflowRepository,
        IEnvironmentRepository environmentRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<CreateWorkflowHandler> logger)
    {
        _workflowRepository = workflowRepository;
        _environmentRepository = environmentRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseDto<WorkflowDto>> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating workflow: {WorkflowName} for environment: {EnvironmentId}", request.Name, request.EnvironmentId);

            // Validate environment exists
            var environment = await _environmentRepository.GetByIdAsync(request.EnvironmentId, cancellationToken);
            if (environment == null || !environment.IsActive)
            {
                throw new NotFoundException("Environment", request.EnvironmentId);
            }

            // Validate owner exists
            var owner = await _userRepository.GetByIdAsync(request.OwnerId, cancellationToken);
            if (owner == null || !owner.IsActive)
            {
                throw new NotFoundException("User", request.OwnerId);
            }

            var workflow = new Workflow
            {
                Name = request.Name,
                Description = request.Description,
                EnvironmentId = request.EnvironmentId,
                OwnerId = request.OwnerId,
                Tags = request.Tags,
                TimeoutMinutes = request.TimeoutMinutes,
                RetryCount = request.RetryCount,
                GlobalVariables = request.GlobalVariables,
                Configuration = request.Configuration,
                Status = WorkflowStatus.Draft,
                IsPublished = false,
                CreatedBy = request.CreatedBy,
                UpdatedBy = request.CreatedBy
            };

            var createdWorkflow = await _workflowRepository.AddAsync(workflow, cancellationToken);
            
            // Load navigation properties for DTO mapping
            var workflowWithNav = await _workflowRepository.GetByIdAsync(createdWorkflow.Id, cancellationToken);
            var workflowDto = _mapper.Map<WorkflowDto>(workflowWithNav);

            _logger.LogInformation("Successfully created workflow with ID: {WorkflowId}", createdWorkflow.Id);

            return new ResponseDto<WorkflowDto>
            {
                Success = true,
                Message = "Workflow created successfully",
                Data = workflowDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow: {WorkflowName}", request.Name);
            return new ResponseDto<WorkflowDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }
}
