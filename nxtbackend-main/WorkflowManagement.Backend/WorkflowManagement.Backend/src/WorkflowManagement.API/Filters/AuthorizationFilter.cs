using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Interfaces.Services;

namespace WorkflowManagement.API.Filters
{
    /// <summary>
    /// Authorization filter for role-based and resource-based authorization
    /// </summary>
    public class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthorizationFilter> _logger;

        public AuthorizationFilter(IUserService userService, ILogger<AuthorizationFilter> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                // Skip authorization for endpoints that allow anonymous access
                if (HasAllowAnonymous(context))
                {
                    return;
                }

                // Check if user is authenticated
                if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                var userId = GetUserId(context.HttpContext.User);
                if (!userId.HasValue)
                {
                    _logger.LogWarning("User ID not found in claims");
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Get user details for authorization checks
                var userResult = await _userService.GetUserByIdAsync(userId.Value);
                if (!userResult.Success || userResult.Data == null)
                {
                    _logger.LogWarning("User {UserId} not found or inactive", userId);
                    context.Result = new UnauthorizedResult();
                    return;
                }

                var user = userResult.Data;

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user {UserId} attempted to access resource", userId);
                    context.Result = new ForbidResult();
                    return;
                }

                // Store user information in HttpContext for use in controllers
                context.HttpContext.Items["CurrentUser"] = user;
                context.HttpContext.Items["CurrentUserId"] = userId.Value;
                context.HttpContext.Items["CurrentUserRole"] = user.Role;

                // Perform role-based authorization
                var hasRequiredRole = await CheckRoleBasedAuthorizationAsync(context, user.Role);
                if (!hasRequiredRole)
                {
                    _logger.LogWarning("User {UserId} with role {Role} denied access to {Action}", 
                        userId, user.Role, context.ActionDescriptor.DisplayName);
                    context.Result = new ForbidResult();
                    return;
                }

                // Perform resource-based authorization
                var hasResourceAccess = await CheckResourceBasedAuthorizationAsync(context, userId.Value, user.Role);
                if (!hasResourceAccess)
                {
                    _logger.LogWarning("User {UserId} denied resource access to {Action}", 
                        userId, context.ActionDescriptor.DisplayName);
                    context.Result = new ForbidResult();
                    return;
                }

                _logger.LogDebug("User {UserId} authorized for {Action}", userId, context.ActionDescriptor.DisplayName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authorization for user {UserId}", GetUserId(context.HttpContext.User));
                context.Result = new StatusCodeResult(500);
            }
        }

        private static bool HasAllowAnonymous(AuthorizationFilterContext context)
        {
            return context.ActionDescriptor.EndpointMetadata.Any(em => em.GetType() == typeof(AllowAnonymousAttribute));
        }

        private static int? GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub") ?? user.FindFirst("userId");
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
        }

        private async Task<bool> CheckRoleBasedAuthorizationAsync(AuthorizationFilterContext context, UserRole userRole)
        {
            // Get required roles from custom attributes
            var requiredRoles = GetRequiredRoles(context);
            
            // If no specific roles required, allow access
            if (!requiredRoles.Any())
            {
                return true;
            }

            // Admin users have access to everything
            if (userRole == UserRole.Admin)
            {
                return true;
            }

            // Check if user role matches any required role
            return requiredRoles.Contains(userRole);
        }

        private async Task<bool> CheckResourceBasedAuthorizationAsync(AuthorizationFilterContext context, int userId, UserRole userRole)
        {
            try
            {
                // Extract resource identifiers from route
                var workflowId = GetRouteValue<int?>(context, "workflowId");
                var projectId = GetRouteValue<int?>(context, "projectId");
                var workspaceId = GetRouteValue<int?>(context, "workspaceId");
                var environmentId = GetRouteValue<int?>(context, "environmentId");
                
                // Admin users have access to all resources
                if (userRole == UserRole.Admin)
                {
                    return true;
                }

                // Check workspace access
                if (workspaceId.HasValue)
                {
                    var hasWorkspaceAccess = await CheckWorkspaceAccessAsync(userId, workspaceId.Value, userRole);
                    if (!hasWorkspaceAccess)
                    {
                        return false;
                    }
                }

                // Check project access
                if (projectId.HasValue)
                {
                    var hasProjectAccess = await CheckProjectAccessAsync(userId, projectId.Value, userRole);
                    if (!hasProjectAccess)
                    {
                        return false;
                    }
                }

                // Check workflow access
                if (workflowId.HasValue)
                {
                    var hasWorkflowAccess = await CheckWorkflowAccessAsync(userId, workflowId.Value, userRole);
                    if (!hasWorkflowAccess)
                    {
                        return false;
                    }
                }

                // Check environment access
                if (environmentId.HasValue)
                {
                    var hasEnvironmentAccess = await CheckEnvironmentAccessAsync(userId, environmentId.Value, userRole);
                    if (!hasEnvironmentAccess)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking resource-based authorization for user {UserId}", userId);
                return false;
            }
        }

        private static List<UserRole> GetRequiredRoles(AuthorizationFilterContext context)
        {
            var requiredRoles = new List<UserRole>();

            // Check for RequireRole attribute
            var roleAttributes = context.ActionDescriptor.EndpointMetadata.OfType<RequireRoleAttribute>();
            foreach (var attr in roleAttributes)
            {
                requiredRoles.AddRange(attr.Roles);
            }

            // If no specific role attributes, check for standard authorize attributes
            var authorizeAttributes = context.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>();
            foreach (var attr in authorizeAttributes)
            {
                if (!string.IsNullOrEmpty(attr.Roles))
                {
                    var roles = attr.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(r => Enum.Parse<UserRole>(r.Trim()))
                                         .ToList();
                    requiredRoles.AddRange(roles);
                }
            }

            return requiredRoles.Distinct().ToList();
        }

        private static T? GetRouteValue<T>(AuthorizationFilterContext context, string key)
        {
            if (context.RouteData.Values.TryGetValue(key, out var value) && value != null)
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return default(T);
                }
            }

            // Also check query parameters
            if (context.HttpContext.Request.Query.TryGetValue(key, out var queryValue))
            {
                try
                {
                    return (T)Convert.ChangeType(queryValue.ToString(), typeof(T));
                }
                catch
                {
                    return default(T);
                }
            }

            return default(T);
        }

        private async Task<bool> CheckWorkspaceAccessAsync(int userId, int workspaceId, UserRole userRole)
        {
            try
            {
                // Project managers and above can access all workspaces
                if (userRole >= UserRole.ProjectManager)
                {
                    return true;
                }

                // Check if user is member of the workspace
                var userWorkspaces = await _userService.GetUserWorkspacesAsync(userId);
                return userWorkspaces.Success && 
                       userWorkspaces.Data?.Any(w => w.WorkspaceId == workspaceId && w.IsActive) == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking workspace access for user {UserId} and workspace {WorkspaceId}", userId, workspaceId);
                return false;
            }
        }

        private async Task<bool> CheckProjectAccessAsync(int userId, int projectId, UserRole userRole)
        {
            try
            {
                // Project managers and above can access all projects
                if (userRole >= UserRole.ProjectManager)
                {
                    return true;
                }

                // Check if user has access to the project through workspace membership
                var userProjects = await _userService.GetUserProjectsAsync(userId);
                return userProjects.Success && 
                       userProjects.Data?.Any(p => p.Id == projectId) == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking project access for user {UserId} and project {ProjectId}", userId, projectId);
                return false;
            }
        }

        private async Task<bool> CheckWorkflowAccessAsync(int userId, int workflowId, UserRole userRole)
        {
            try
            {
                // Project managers and above can access all workflows
                if (userRole >= UserRole.ProjectManager)
                {
                    return true;
                }

                // Check if user has access to the workflow through project membership
                var userWorkflows = await _userService.GetUserWorkflowsAsync(userId);
                return userWorkflows.Success && 
                       userWorkflows.Data?.Any(w => w.Id == workflowId) == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking workflow access for user {UserId} and workflow {WorkflowId}", userId, workflowId);
                return false;
            }
        }

        private async Task<bool> CheckEnvironmentAccessAsync(int userId, int environmentId, UserRole userRole)
        {
            try
            {
                // Project managers and above can access all environments
                if (userRole >= UserRole.ProjectManager)
                {
                    return true;
                }

                // Check if user has access to the environment through project membership
                var userEnvironments = await _userService.GetUserEnvironmentsAsync(userId);
                return userEnvironments.Success && 
                       userEnvironments.Data?.Any(e => e.Id == environmentId) == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking environment access for user {UserId} and environment {EnvironmentId}", userId, environmentId);
                return false;
            }
        }
    }

    /// <summary>
    /// Attribute to specify required roles for an action
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRoleAttribute : Attribute
    {
        public List<UserRole> Roles { get; }

        public RequireRoleAttribute(params UserRole[] roles)
        {
            Roles = roles?.ToList() ?? new List<UserRole>();
        }
    }

    /// <summary>
    /// Attribute to specify required permissions for an action
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequirePermissionAttribute : Attribute
    {
        public List<string> Permissions { get; }

        public RequirePermissionAttribute(params string[] permissions)
        {
            Permissions = permissions?.ToList() ?? new List<string>();
        }
    }

    /// <summary>
    /// Attribute to specify resource ownership requirement
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireOwnershipAttribute : Attribute
    {
        public string ResourceType { get; }
        public string ResourceIdParameter { get; }

        public RequireOwnershipAttribute(string resourceType, string resourceIdParameter = "id")
        {
            ResourceType = resourceType;
            ResourceIdParameter = resourceIdParameter;
        }
    }
}