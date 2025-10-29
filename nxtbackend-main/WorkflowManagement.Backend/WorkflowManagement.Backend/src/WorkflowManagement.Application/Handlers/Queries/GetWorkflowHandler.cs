// GetWorkflowHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.Queries.Workflows;
using WorkflowManagement.Core.Exceptions;
using WorkflowManagement.Core.Interfaces.Repositories;

namespace WorkflowManagement.Application.Handlers.Queries;

public class GetWorkflowHandler : IRequestHandler<GetWorkflowQuery, ResponseDto<WorkflowDto>>
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetWorkflowHandler> _logger;

    public GetWorkflowHandler(IWorkflowRepository workflowRepository, IMapper mapper, ILogger<GetWorkflowHandler> logger)
    {
        _workflowRepository = workflowRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseDto<WorkflowDto>> Handle(GetWorkflowQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting workflow with ID: {WorkflowId}", request.Id);

            var workflow = request.IncludeSteps
                ? await _workflowRepository.GetWithStepsAsync(request.Id, cancellationToken)
                : request.IncludeVersions
                    ? await _workflowRepository.GetWithVersionsAsync(request.Id, cancellationToken)
                    : await _workflowRepository.GetByIdAsync(request.Id, cancellationToken);

            if (workflow == null)
            {
                throw new NotFoundException("Workflow", request.Id);
            }

            var workflowDto = _mapper.Map<WorkflowDto>(workflow);

            return new ResponseDto<WorkflowDto>
            {
                Success = true,
                Message = "Workflow retrieved successfully",
                Data = workflowDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow with ID: {WorkflowId}", request.Id);
            return new ResponseDto<WorkflowDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }
}