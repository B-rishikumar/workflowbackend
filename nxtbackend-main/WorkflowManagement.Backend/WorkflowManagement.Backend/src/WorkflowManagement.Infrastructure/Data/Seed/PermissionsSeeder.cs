using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Infrastructure.Data.Context;

namespace WorkflowManagement.Infrastructure.Data.Seed;

public class PermissionsSeeder
{
    private readonly WorkflowDbContext _context;
    private readonly ILogger<PermissionsSeeder> _logger;

    public PermissionsSeeder(WorkflowDbContext context, ILogger<PermissionsSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedPermissionsAsync()
    {
        _logger.LogInformation("Starting permissions seeding...");

        if (await _context.Permissions.AnyAsync())
        {
            _logger.LogInformation("Permissions already exist. Skipping seeding.");
            return;
        }

        var permissions = CreateSystemPermissions();
        await _context.Permissions.AddRangeAsync(permissions);

        var roles = await CreateSystemRolesAsync();
        await _context.Roles.AddRangeAsync(roles);

        await _context.SaveChangesAsync();

        // Assign permissions to roles
        await AssignPermissionsToRolesAsync();

        _logger.LogInformation("Permissions seeding completed successfully.");
    }

    private List<Permission> CreateSystemPermissions()
    {
        var permissions = new List<Permission>();
        var sortOrder = 1;

        // User Management Permissions
        permissions.AddRange(new[]
        {
            CreatePermission("Users", "View", "List", "View users list", "User Management", sortOrder++),
            CreatePermission("Users", "View", "Details", "View user details", "User Management", sortOrder++),
            CreatePermission("Users", "Create", null, "Create new users", "User Management", sortOrder++),
            CreatePermission("Users", "Update", null, "Update user information", "User Management", sortOrder++),
            CreatePermission("Users", "Delete", null, "Delete users", "User Management", sortOrder++),
            CreatePermission("Users", "Manage", "Roles", "Manage user roles", "User Management", sortOrder++),
            CreatePermission("Users", "Reset", "Password", "Reset user passwords", "User Management", sortOrder++),
            CreatePermission("Users", "Lock", null, "Lock/unlock user accounts", "User Management", sortOrder++)
        });

        // Workspace Management Permissions
        permissions.AddRange(new[]
        {
            CreatePermission("Workspaces", "View", "List", "View workspaces list", "Workspace Management", sortOrder++),
            CreatePermission("Workspaces", "View", "Details", "View workspace details", "Workspace Management", sortOrder++),
            CreatePermission("Workspaces", "Create", null, "Create new workspaces", "Workspace Management", sortOrder++),
            CreatePermission("Workspaces", "Update", null, "Update workspace information", "Workspace Management", sortOrder++),
            CreatePermission("Workspaces", "Delete", null, "Delete workspaces", "Workspace Management", sortOrder++),
            CreatePermission("Workspaces", "Manage", "Members", "Manage workspace members", "Workspace Management", sortOrder++)
        });

        // Project Management Permissions
        permissions.AddRange(new[]
        {
            CreatePermission("Projects", "View", "List", "View projects list", "Project Management", sortOrder++),
            CreatePermission("Projects", "View", "Details", "View project details", "Project Management", sortOrder++),
            CreatePermission("Projects", "Create", null, "Create new projects", "Project Management", sortOrder++),
            CreatePermission("Projects", "Update", null, "Update project information", "Project Management", sortOrder++),
            CreatePermission("Projects", "Delete", null, "Delete projects", "Project Management", sortOrder++),
            CreatePermission("Projects", "Manage", "Settings", "Manage project settings", "Project Management", sortOrder++)
        });

        // Environment Management Permissions
        permissions.AddRange(new[]
        {
            CreatePermission("Environments", "View", "List", "View environments list", "Environment Management", sortOrder++),
            CreatePermission("Environments", "View", "Details", "View environment details", "Environment Management", sortOrder++),
            CreatePermission("Environments", "Create", null, "Create new environments", "Environment Management", sortOrder++),
            CreatePermission("Environments", "Update", null, "Update environment information", "Environment Management", sortOrder++),
            CreatePermission("Environments", "Delete", null, "Delete environments", "Environment Management", sortOrder++),
            CreatePermission("Environments", "Manage", "Variables", "Manage environment variables", "Environment Management", sortOrder++)
        });

        // Workflow Management Permissions
        permissions.AddRange(new[]
        {
            CreatePermission("Workflows", "View", "List", "View workflows list", "Workflow Management", sortOrder++),
            CreatePermission("Workflows", "View", "Details", "View workflow details", "Workflow Management", sortOrder++),
            CreatePermission("Workflows", "Create", null, "Create new workflows", "Workflow Management", sortOrder++),
            CreatePermission("Workflows", "Update", null, "Update workflow information", "Workflow Management", sortOrder++),
            CreatePermission("Workflows", "Delete", null, "Delete workflows", "Workflow Management", sortOrder++),
            CreatePermission("Workflows", "Execute", "Manual", "Execute workflows manually", "Workflow Management", sortOrder++),
            CreatePermission("Workflows", "Execute", "Scheduled", "Execute workflows on schedule", "Workflow Management", sortOrder++),
            CreatePermission("Workflows", "Publish", null, "Publish workflows", "Workflow Management", sortOrder++),
            CreatePermission("Workflows", "Manage", "Versions", "Manage workflow versions", "Workflow Management", sortOrder++),
            CreatePermission("Workflows", "Manage", "Steps", "Manage workflow steps", "Workflow Management", sortOrder++)
        });

        // API Endpoint Management Permissions
        permissions.AddRange(new[]
        {
            CreatePermission("ApiEndpoints", "View", "List", "View API endpoints list", "API Management", sortOrder++),
            CreatePermission("ApiEndpoints", "View", "Details", "View API endpoint details", "API Management", sortOrder++),
            CreatePermission("ApiEndpoints", "Create", null, "Create new API endpoints", "API Management", sortOrder++),
            CreatePermission("ApiEndpoints", "Update", null, "Update API endpoint information", "API Management", sortOrder++),
            CreatePermission("ApiEndpoints", "Delete", null, "Delete API endpoints", "API Management", sortOrder++),
            CreatePermission("ApiEndpoints", "Test", null, "Test API endpoints", "API Management", sortOrder++),
            CreatePermission("ApiEndpoints", "Import", "Swagger", "Import Swagger definitions", "API Management", sortOrder++),
            CreatePermission("ApiEndpoints", "Import", "WSDL", "Import WSDL definitions", "API Management", sortOrder++)
        });

        // Workflow Execution Permissions
        permissions.AddRange(new[]
        {
            CreatePermission("Executions", "View", "List", "View workflow executions list", "Execution Management", sortOrder++),
            CreatePermission("Executions", "View", "Details", "View execution details", "Execution Management", sortOrder++),
            CreatePermission("Executions", "View", "Logs", "View execution logs", "Execution Management", sortOrder++),
            CreatePermission("Executions", "Cancel", null, "Cancel running executions", "Execution Management", sortOrder++),
            CreatePermission("Executions", "Retry", null, "Retry failed executions", "Execution Management", sortOrder++),
            CreatePermission("Executions", "Delete", null, "Delete execution records", "Execution Management", sortOrder++)
        });

        // Approval Management Permissions
        permissions.AddRange(new[]
        {
            CreatePermission("Approvals", "View", "List", "View approval requests list", "Approval Management", sortOrder++),
            CreatePermission("Approvals", "View", "Details", "View approval details", "Approval Management", sortOrder++),
            CreatePermission("Approvals", "Create", null, "Create approval requests", "Approval Management", sortOrder++),
            CreatePermission("Approvals", "Process", "Approve", "Approve requests", "Approval Management", sortOrder++),
            CreatePermission("Approvals", "Process", "Reject", "Reject requests", "Approval Management", sortOrder++),
            CreatePermission("Approvals", "Cancel", null, "Cancel approval requests", "Approval Management", sortOrder++)
        });

        // Scheduling Permissions
        permissions.AddRange(new[]
        {
            CreatePermission("Schedules", "View", "List", "View workflow schedules list", "Schedule Management", sortOrder++),
            CreatePermission("Schedules", "View", "Details", "View schedule details", "Schedule Management", sortOrder++),
            CreatePermission("Schedules", "Create", null, "Create workflow schedules", "Schedule Management", sortOrder++),
            CreatePermission("Schedules", "Update", null, "Update workflow schedules", "Schedule Management", sortOrder++),
            CreatePermission("Schedules", "Delete", null, "Delete workflow schedules", "Schedule Management", sortOrder++),
            CreatePermission("Schedules", "Enable", null, "Enable workflow schedules", "Schedule Management", sortOrder++),
            CreatePermission("Schedules", "Disable", null, "Disable workflow schedules", "Schedule Management", sortOrder++)
        });

        // Metrics and Reporting Permissions
        permissions.AddRange(new[]
        {
            CreatePermission("Metrics", "View", "Dashboard", "View metrics dashboard", "Analytics", sortOrder++),
            CreatePermission("Metrics", "View", "Reports", "View detailed reports", "Analytics", sortOrder++),
            CreatePermission("Metrics", "Export", "Data", "Export metrics data", "Analytics", sortOrder++),
            CreatePermission("Reports", "Generate", "System", "Generate system reports", "Analytics", sortOrder++),
            CreatePermission("Reports", "Generate", "Custom", "Generate custom reports", "Analytics", sortOrder++),
            CreatePermission("Audit", "View", "Logs", "View audit logs", "Analytics", sortOrder++)
        });

        // System Administration Permissions
        permissions.AddRange(new[]
        {
            CreatePermission("System", "View", "Settings", "View system settings", "System Administration", sortOrder++),
            CreatePermission("System", "Update", "Settings", "Update system settings", "System Administration", sortOrder++),
            CreatePermission("System", "View", "Health", "View system health", "System Administration", sortOrder++),
            CreatePermission("System", "Manage", "Cache", "Manage system cache", "System Administration", sortOrder++),
            CreatePermission("System", "Manage", "Jobs", "Manage background jobs", "System Administration", sortOrder++),
            CreatePermission("System", "Access", "Logs", "Access system logs", "System Administration", sortOrder++)
        });

        return permissions;
    }

    private Permission CreatePermission(string module, string action, string? resource, string description, string category, int sortOrder)
    {
        var name = $"{module}.{action}" + (string.IsNullOrEmpty(resource) ? "" : $".{resource}");
        
        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Module = module,
            Action = action,
            Resource = resource,
            Category = category,
            SortOrder = sortOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };
    }

    private async Task<List<Role>> CreateSystemRolesAsync()
    {
        var roles = new List<Role>
        {
            new Role
            {
                Id = Guid.Parse("ROLE0001-0001-0001-0001-000000000001"),
                Name = "Super Administrator",
                NormalizedName = "SUPER ADMINISTRATOR",
                Description = "Full system access with all permissions",
                IsActive = true,
                IsSystemRole = true,
                Priority = 1,
                Color = "#e74c3c",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Role
            {
                Id = Guid.Parse("ROLE0002-0002-0002-0002-000000000002"),
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR",
                Description = "System administration with most permissions",
                IsActive = true,
                IsSystemRole = true,
                Priority = 2,
                Color = "#f39c12",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Role
            {
                Id = Guid.Parse("ROLE0003-0003-0003-0003-000000000003"),
                Name = "Manager",
                NormalizedName = "MANAGER",
                Description = "Management role with approval and oversight permissions",
                IsActive = true,
                IsSystemRole = true,
                Priority = 3,
                Color = "#3498db",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Role
            {
                Id = Guid.Parse("ROLE0004-0004-0004-0004-000000000004"),
                Name = "Developer",
                NormalizedName = "DEVELOPER",
                Description = "Developer role with workflow creation and execution permissions",
                IsActive = true,
                IsSystemRole = true,
                Priority = 4,
                Color = "#2ecc71",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Role
            {
                Id = Guid.Parse("ROLE0005-0005-0005-0005-000000000005"),
                Name = "Viewer",
                NormalizedName = "VIEWER",
                Description = "Read-only access to workflows and executions",
                IsActive = true,
                IsSystemRole = true,
                Priority = 5,
                Color = "#95a5a6",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        return roles;
    }

    private async Task AssignPermissionsToRolesAsync()
    {
        _logger.LogInformation("Assigning permissions to roles...");

        var permissions = await _context.Permissions.ToListAsync();
        var roles = await _context.Roles.ToListAsync();

        var rolePermissions = new List<RolePermission>();

        // Super Administrator - All permissions
        var superAdminRole = roles.First(r => r.Name == "Super Administrator");
        foreach (var permission in permissions)
        {
            rolePermissions.Add(CreateRolePermission(superAdminRole.Id, permission.Id));
        }

        // Administrator - All except critical system operations
        var adminRole = roles.First(r => r.Name == "Administrator");
        var adminExcludedPermissions = new[]
        {
            "System.Update.Settings",
            "System.Manage.Jobs",
            "Users.Delete"
        };

        foreach (var permission in permissions.Where(p => !adminExcludedPermissions.Contains(p.Name)))
        {
            rolePermissions.Add(CreateRolePermission(adminRole.Id, permission.Id));
        }

        // Manager - Management and oversight permissions
        var managerRole = roles.First(r => r.Name == "Manager");
        var managerPermissions = new[]
        {
            // Viewing permissions
            "Users.View.List", "Users.View.Details",
            "Workspaces.View.List", "Workspaces.View.Details",
            "Projects.View.List", "Projects.View.Details", "Projects.Create", "Projects.Update",
            "Environments.View.List", "Environments.View.Details", "Environments.Create", "Environments.Update",
            "Workflows.View.List", "Workflows.View.Details", "Workflows.Create", "Workflows.Update",
            "ApiEndpoints.View.List", "ApiEndpoints.View.Details",
            
            // Execution management
            "Executions.View.List", "Executions.View.Details", "Executions.View.Logs",
            "Workflows.Execute.Manual", "Workflows.Execute.Scheduled",
            
            // Approval permissions
            "Approvals.View.List", "Approvals.View.Details", "Approvals.Process.Approve", "Approvals.Process.Reject",
            
            // Scheduling
            "Schedules.View.List", "Schedules.View.Details", "Schedules.Create", "Schedules.Update", "Schedules.Enable", "Schedules.Disable",
            
            // Analytics
            "Metrics.View.Dashboard", "Metrics.View.Reports", "Reports.Generate.System"
        };

        foreach (var permissionName in managerPermissions)
        {
            var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
            if (permission != null)
            {
                rolePermissions.Add(CreateRolePermission(managerRole.Id, permission.Id));
            }
        }

        // Developer - Development and execution permissions
        var developerRole = roles.First(r => r.Name == "Developer");
        var developerPermissions = new[]
        {
            // Viewing permissions
            "Workspaces.View.List", "Workspaces.View.Details",
            "Projects.View.List", "Projects.View.Details",
            "Environments.View.List", "Environments.View.Details",
            
            // Full workflow management
            "Workflows.View.List", "Workflows.View.Details", "Workflows.Create", "Workflows.Update", "Workflows.Delete",
            "Workflows.Execute.Manual", "Workflows.Manage.Steps", "Workflows.Manage.Versions",
            
            // API endpoint management
            "ApiEndpoints.View.List", "ApiEndpoints.View.Details", "ApiEndpoints.Create", "ApiEndpoints.Update", "ApiEndpoints.Delete",
            "ApiEndpoints.Test", "ApiEndpoints.Import.Swagger", "ApiEndpoints.Import.WSDL",
            
            // Execution management
            "Executions.View.List", "Executions.View.Details", "Executions.View.Logs", "Executions.Retry",
            
            // Approval creation
            "Approvals.Create", "Approvals.View.List", "Approvals.View.Details",
            
            // Basic scheduling
            "Schedules.View.List", "Schedules.View.Details", "Schedules.Create", "Schedules.Update",
            
            // Basic analytics
            "Metrics.View.Dashboard", "Metrics.View.Reports"
        };

        foreach (var permissionName in developerPermissions)
        {
            var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
            if (permission != null)
            {
                rolePermissions.Add(CreateRolePermission(developerRole.Id, permission.Id));
            }
        }

        // Viewer - Read-only permissions
        var viewerRole = roles.First(r => r.Name == "Viewer");
        var viewerPermissions = new[]
        {
            // View-only permissions
            "Workspaces.View.List", "Workspaces.View.Details",
            "Projects.View.List", "Projects.View.Details",
            "Environments.View.List", "Environments.View.Details",
            "Workflows.View.List", "Workflows.View.Details",
            "ApiEndpoints.View.List", "ApiEndpoints.View.Details",
            "Executions.View.List", "Executions.View.Details", "Executions.View.Logs",
            "Schedules.View.List", "Schedules.View.Details",
            "Metrics.View.Dashboard", "Metrics.View.Reports"
        };

        foreach (var permissionName in viewerPermissions)
        {
            var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
            if (permission != null)
            {
                rolePermissions.Add(CreateRolePermission(viewerRole.Id, permission.Id));
            }
        }

        await _context.RolePermissions.AddRangeAsync(rolePermissions);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned {Count} permissions to roles", rolePermissions.Count);
    }

    private RolePermission CreateRolePermission(Guid roleId, Guid permissionId)
    {
        return new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId,
            IsGranted = true,
            GrantedAt = DateTime.UtcNow,
            GrantedBy = "System",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };
    }

    public async Task UpdateUserRolesAsync()
    {
        _logger.LogInformation("Updating existing users with detailed roles...");

        var users = await _context.Users.ToListAsync();
        var roles = await _context.Roles.ToListAsync();

        foreach (var user in users)
        {
            var roleName = user.Role.ToString();
            var detailedRole = roles.FirstOrDefault(r => r.Name.Replace(" ", "") == roleName);
            
            if (detailedRole != null)
            {
                user.RoleId = detailedRole.Id;
                _context.Users.Update(user);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated {Count} users with detailed roles", users.Count);
    }

    public async Task<List<Permission>> GetPermissionsByCategoryAsync(string category)
    {
        return await _context.Permissions
            .Where(p => p.Category == category && p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();
    }

    public async Task<List<string>> GetAllCategoriesAsync()
    {
        return await _context.Permissions
            .Where(p => p.IsActive)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName)
    {
        var user = await _context.Users
            .Include(u => u.DetailedRole)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.DetailedRole == null)
            return false;

        return user.DetailedRole.Permissions.Any(p => p.Name == permissionName && p.IsActive);
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.DetailedRole)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.DetailedRole == null)
            return new List<string>();

        return user.DetailedRole.Permissions
            .Where(p => p.IsActive)
            .Select(p => p.Name)
            .ToList();
    }

    public async Task GrantPermissionToRoleAsync(Guid roleId, Guid permissionId, string grantedBy)
    {
        var existingRolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (existingRolePermission == null)
        {
            var rolePermission = new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                PermissionId = permissionId,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow,
                GrantedBy = grantedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = grantedBy,
                UpdatedBy = grantedBy
            };

            await _context.RolePermissions.AddAsync(rolePermission);
        }
        else
        {
            existingRolePermission.IsGranted = true;
            existingRolePermission.GrantedAt = DateTime.UtcNow;
            existingRolePermission.GrantedBy = grantedBy;
            existingRolePermission.UpdatedAt = DateTime.UtcNow;
            existingRolePermission.UpdatedBy = grantedBy;
        }

        await _context.SaveChangesAsync();
    }

    public async Task RevokePermissionFromRoleAsync(Guid roleId, Guid permissionId, string revokedBy)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission != null)
        {
            rolePermission.IsGranted = false;
            rolePermission.UpdatedAt = DateTime.UtcNow;
            rolePermission.UpdatedBy = revokedBy;
            rolePermission.Notes = $"Permission revoked by {revokedBy} on {DateTime.UtcNow}";

            await _context.SaveChangesAsync();
        }
    }
}