// GetWorkflowsHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.Queries.Workflows;
using WorkflowManagement.Core.Interfaces.Repositories;

namespace WorkflowManagement.Application.Handlers.Queries;

public class GetWorkflowsHandler : IRequestHandler<GetWorkflowsQuery, ResponseDto<PagedResultDto<WorkflowDto>>>
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetWorkflowsHandler> _logger;

    public GetWorkflowsHandler(IWorkflowRepository workflowRepository, IMapper mapper, ILogger<GetWorkflowsHandler> logger)
    {
        _workflowRepository = workflowRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseDto<PagedResultDto<WorkflowDto>>> Handle(GetWorkflowsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting workflows with page: {PageNumber}, size: {PageSize}", request.PageNumber, request.PageSize);

            var (workflows, totalCount) = await _workflowRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                BuildWorkflowPredicate(request),
                orderBy: w => w.UpdatedAt,
                ascending: false,
                cancellationToken: cancellationToken);

            var workflowDtos = _mapper.Map<IEnumerable<WorkflowDto>>(workflows);

            var pagedResult = new PagedResultDto<WorkflowDto>
            {
                Items = workflowDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ResponseDto<PagedResultDto<WorkflowDto>>
            {
                Success = true,
                Message = $"Retrieved {workflowDtos.Count()} workflows",
                Data = pagedResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflows");
            return new ResponseDto<PagedResultDto<WorkflowDto>>
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }

    private System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.Workflow, bool>>? BuildWorkflowPredicate(GetWorkflowsQuery request)
    {
        System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.Workflow, bool>>? predicate = null;

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var lowerSearchTerm = request.SearchTerm.ToLower();
            predicate = w => w.Name.ToLower().Contains(lowerSearchTerm) ||
                           (w.Description != null && w.Description.ToLower().Contains(lowerSearchTerm)) ||
                           (w.Tags != null && w.Tags.ToLower().Contains(lowerSearchTerm));
        }

        if (request.EnvironmentId.HasValue)
        {
            var envPredicate = System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.Workflow, bool>>(
                System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.Workflow), "w"), 
                        "EnvironmentId"),
                    System.Linq.Expressions.Expression.Constant(request.EnvironmentId.Value)),
                System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.Workflow), "w"));
            
            predicate = predicate == null ? envPredicate : CombineWorkflowPredicates(predicate, envPredicate);
        }

        if (request.OwnerId.HasValue)
        {
            var ownerPredicate = System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.Workflow, bool>>(
                System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.Workflow), "w"), 
                        "OwnerId"),
                    System.Linq.Expressions.Expression.Constant(request.OwnerId.Value)),
                System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.Workflow), "w"));
            
            predicate = predicate == null ? ownerPredicate : CombineWorkflowPredicates(predicate, ownerPredicate);
        }

        if (request.Status.HasValue)
        {
            var statusPredicate = System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.Workflow, bool>>(
                System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.Workflow), "w"), 
                        "Status"),
                    System.Linq.Expressions.Expression.Constant(request.Status.Value)),
                System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.Workflow), "w"));
            
            predicate = predicate == null ? statusPredicate : CombineWorkflowPredicates(predicate, statusPredicate);
        }

        if (request.IsPublished.HasValue)
        {
            var publishedPredicate = System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.Workflow, bool>>(
                System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.Workflow), "w"), 
                        "IsPublished"),
                    System.Linq.Expressions.Expression.Constant(request.IsPublished.Value)),
                System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.Workflow), "w"));
            
            predicate = predicate == null ? publishedPredicate : CombineWorkflowPredicates(predicate, publishedPredicate);
        }

        return predicate;
    }

    private System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.Workflow, bool>> CombineWorkflowPredicates(
        System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.Workflow, bool>> first,
        System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.Workflow, bool>> second)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.Workflow), "w");
        var leftVisitor = new WorkflowParameterReplacer(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);
        var rightVisitor = new WorkflowParameterReplacer(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);
        return System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.Workflow, bool>>(
            System.Linq.Expressions.Expression.AndAlso(left!, right!), parameter);
    }
}
