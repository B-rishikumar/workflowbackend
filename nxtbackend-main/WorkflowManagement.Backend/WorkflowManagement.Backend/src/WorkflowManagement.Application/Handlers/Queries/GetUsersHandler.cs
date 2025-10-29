// GetUsersHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Application.DTOs.User;
using WorkflowManagement.Application.Queries.Users;
using WorkflowManagement.Core.Interfaces.Repositories;

namespace WorkflowManagement.Application.Handlers.Queries;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, ResponseDto<PagedResultDto<UserDto>>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUsersHandler> _logger;

    public GetUsersHandler(IUserRepository userRepository, IMapper mapper, ILogger<GetUsersHandler> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseDto<PagedResultDto<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting users with page: {PageNumber}, size: {PageSize}", request.PageNumber, request.PageSize);

            var (users, totalCount) = await _userRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                BuildPredicate(request),
                orderBy: u => u.CreatedAt,
                ascending: false,
                cancellationToken: cancellationToken);

            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

            var pagedResult = new PagedResultDto<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ResponseDto<PagedResultDto<UserDto>>
            {
                Success = true,
                Message = $"Retrieved {userDtos.Count()} users",
                Data = pagedResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return new ResponseDto<PagedResultDto<UserDto>>
            {
                Success = false,
                Message = ex.Message,
                Errors = new[] { ex.Message }
            };
        }
    }

    private System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.User, bool>>? BuildPredicate(GetUsersQuery request)
    {
        System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.User, bool>>? predicate = null;

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var lowerSearchTerm = request.SearchTerm.ToLower();
            predicate = u => u.FirstName.ToLower().Contains(lowerSearchTerm) ||
                           u.LastName.ToLower().Contains(lowerSearchTerm) ||
                           u.Email.ToLower().Contains(lowerSearchTerm) ||
                           u.Username.ToLower().Contains(lowerSearchTerm);
        }

        if (request.Role.HasValue)
        {
            var rolePredicate = System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.User, bool>>(
                System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.User), "u"), 
                        "Role"),
                    System.Linq.Expressions.Expression.Constant(request.Role.Value)),
                System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.User), "u"));
            
            predicate = predicate == null ? rolePredicate : CombinePredicates(predicate, rolePredicate);
        }

        if (request.IsActive.HasValue)
        {
            var activePredicate = System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.User, bool>>(
                System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.User), "u"), 
                        "IsActive"),
                    System.Linq.Expressions.Expression.Constant(request.IsActive.Value)),
                System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.User), "u"));
            
            predicate = predicate == null ? activePredicate : CombinePredicates(predicate, activePredicate);
        }

        if (!string.IsNullOrEmpty(request.Department))
        {
            var deptPredicate = System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.User, bool>>(
                System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.User), "u"), 
                        "Department"),
                    System.Linq.Expressions.Expression.Constant(request.Department)),
                System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.User), "u"));
            
            predicate = predicate == null ? deptPredicate : CombinePredicates(predicate, deptPredicate);
        }

        return predicate;
    }

    private System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.User, bool>> CombinePredicates(
        System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.User, bool>> first,
        System.Linq.Expressions.Expression<Func<WorkflowManagement.Core.Entities.User, bool>> second)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(WorkflowManagement.Core.Entities.User), "u");
        var leftVisitor = new ParameterReplacer(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);
        var rightVisitor = new ParameterReplacer(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);
        return System.Linq.Expressions.Expression.Lambda<Func<WorkflowManagement.Core.Entities.User, bool>>(
            System.Linq.Expressions.Expression.AndAlso(left!, right!), parameter);
    }
}
