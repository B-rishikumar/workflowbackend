// GetWorkflowExecutionsHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.Queries.Workflows;
using WorkflowManagement.Core.Interfaces.Repositories;

namespace WorkflowManagement.Application.Handlers.Queries;

public class GetWorkflowExecutionsHandler : IRequestHandler<GetWorkflowExecutionsQuery, ResponseDto<PagedResultDto<WorkflowExecutionDto>>>
{
    private readonly IWorkflowExecutionRepository _executionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetWorkflowExecutionsHandler> _logger;

    public GetWorkflowExecutionsHandler(
        IWorkflowExecutionRepository executionRepository,
        IMapper mapper,
        ILogger<GetWorkflowExecutionsHandler> logger)
    {
        _executionRepository = executionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseDto<PagedResultDto<WorkflowExecutionDto>>> Handle(GetWorkflowExecutionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting workflow executions with page: {PageNumber}, size: {PageSize}", request.PageNumber, request.PageSize);

            var (executions, totalCount) = await _executionRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                BuildExecutionPredicate(request),
                orderBy: e => e.CreatedAt,
                ascending: false,
                cancellationToken: cancellationToken);

            var executionDtos = _mapper.Map<IEnumerable<WorkflowExecutionDto>>(executions);

            var pagedResult = new PagedResultDto<WorkflowExecutionDto>
            {
                Items = executionDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ResponseDto<PagedResultDto<WorkflowExecutionDto>>
            {
                Success = true,
                Message = $"Retrieved {executionDtos.Count()} workflow executions",
                Data = pagedResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow executions");
            return new ResponseDto<PagedResultDto<WorkflowExecutionDto>>
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }

    private System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.WorkflowExecution, bool>>? BuildExecutionPredicate(GetWorkflowExecutionsQuery request)
    {
        System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.WorkflowExecution, bool>>? predicate = null;

        if (request.WorkflowId.HasValue)
        {
            predicate = e => e.WorkflowId == request.WorkflowId.Value;
        }

        if (request.ExecutedById.HasValue)
        {
            var executedByPredicate = System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.WorkflowExecution, bool>>(
                System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.WorkflowExecution), "e"), 
                        "ExecutedById"),
                    System.Linq.Expressions.Expression.Constant(request.ExecutedById.Value)),
                System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.WorkflowExecution), "e"));
            
            predicate = predicate == null ? executedByPredicate : CombineExecutionPredicates(predicate, executedByPredicate);
        }

        if (request.Status.HasValue)
        {
            var statusPredicate = System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.WorkflowExecution, bool>>(
                System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.WorkflowExecution), "e"), 
                        "Status"),
                    System.Linq.Expressions.Expression.Constant(request.Status.Value)),
                System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.WorkflowExecution), "e"));
            
            predicate = predicate == null ? statusPredicate : CombineExecutionPredicates(predicate, statusPredicate);
        }

        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            var startDate = request.StartDate ?? DateTime.MinValue;
            var endDate = request.EndDate ?? DateTime.MaxValue;
            
            var datePredicate = System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.WorkflowExecution, bool>>(
                System.Linq.Expressions.Expression.AndAlso(
                    System.Linq.Expressions.Expression.GreaterThanOrEqual(
                        System.Linq.Expressions.Expression.Property(
                            System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.WorkflowExecution), "e"), 
                            "CreatedAt"),
                        System.Linq.Expressions.Expression.Constant(startDate)),
                    System.Linq.Expressions.Expression.LessThanOrEqual(
                        System.Linq.Expressions.Expression.Property(
                            System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.WorkflowExecution), "e"), 
                            "CreatedAt"),
                        System.Linq.Expressions.Expression.Constant(endDate))),
                System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.WorkflowExecution), "e"));
            
            predicate = predicate == null ? datePredicate : CombineExecutionPredicates(predicate, datePredicate);
        }

        if (!string.IsNullOrEmpty(request.TriggerType))
        {
            var triggerPredicate = System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.WorkflowExecution, bool>>(
                System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.WorkflowExecution), "e"), 
                        "TriggerType"),
                    System.Linq.Expressions.Expression.Constant(request.TriggerType)),
                System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.WorkflowExecution), "e"));
            
            predicate = predicate == null ? triggerPredicate : CombineExecutionPredicates(predicate, triggerPredicate);
        }

        return predicate;
    }

    private System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.WorkflowExecution, bool>> CombineExecutionPredicates(
        System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.WorkflowExecution, bool>> first,
        System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.WorkflowExecution, bool>> second)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.WorkflowExecution), "e");
        var leftVisitor = new ExecutionParameterReplacer(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);
        var rightVisitor = new ExecutionParameterReplacer(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);
        return System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.WorkflowExecution, bool>>(
            System.Linq.Expressions.Expression.AndAlso(left!, right!), parameter);
    }
}