// DataSeeder.csusing Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Infrastructure.Data.Context;
using WorkflowManagement.Shared.Helpers;

namespace WorkflowManagement.Infrastructure.Data.Seed;

public class DataSeeder
{
    private readonly WorkflowDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(WorkflowDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Database already contains data. Skipping seeding.");
                return;
            }

            // Seed in order due to foreign key dependencies
            await SeedUsersAsync();
            await SeedWorkspacesAsync();
            await SeedProjectsAsync();
            await SeedEnvironmentsAsync();
            await SeedApiEndpointsAsync();
            await SeedWorkflowsAsync();
            await SeedWorkflowStepsAsync();
            await SeedSampleExecutionsAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database seeding");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        _logger.LogInformation("Seeding users...");

        var users = new List<User>
        {
            new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@workflowmanagement.com",
                Username = "admin",
                PasswordHash = CryptographyHelper.HashPassword("Admin@123"),
                Role = UserRole.SuperAdmin,
                IsActive = true,
                EmailConfirmed = true,
                Department = "IT",
                JobTitle = "System Administrator",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new User
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "John",
                LastName = "Manager",
                Email = "john.manager@workflowmanagement.com",
                Username = "jmanager",
                PasswordHash = CryptographyHelper.HashPassword("Manager@123"),
                Role = UserRole.Manager,
                IsActive = true,
                EmailConfirmed = true,
                Department = "Operations",
                JobTitle = "Operations Manager",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new User
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                FirstName = "Alice",
                LastName = "Developer",
                Email = "alice.developer@workflowmanagement.com",
                Username = "adeveloper",
                PasswordHash = CryptographyHelper.HashPassword("Developer@123"),
                Role = UserRole.Developer,
                IsActive = true,
                EmailConfirmed = true,
                Department = "Development",
                JobTitle = "Senior Developer",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new User
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                FirstName = "Bob",
                LastName = "Analyst",
                Email = "bob.analyst@workflowmanagement.com",
                Username = "banalyst",
                PasswordHash = CryptographyHelper.HashPassword("Analyst@123"),
                Role = UserRole.Viewer,
                IsActive = true,
                EmailConfirmed = true,
                Department = "Business Analysis",
                JobTitle = "Business Analyst",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await _context.Users.AddRangeAsync(users);
        _logger.LogInformation("Seeded {Count} users", users.Count);
    }

    private async Task SeedWorkspacesAsync()
    {
        _logger.LogInformation("Seeding workspaces...");

        var workspaces = new List<Workspace>
        {
            new Workspace
            {
                Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                Name = "Default Workspace",
                Description = "Default workspace for getting started with workflow management",
                IsActive = true,
                OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Settings = new Dictionary<string, object>
                {
                    ["theme"] = "default",
                    ["notifications"] = true,
                    ["autoSave"] = true
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Workspace
            {
                Id = Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
                Name = "E-Commerce Integration",
                Description = "Workspace for e-commerce platform integrations and workflows",
                IsActive = true,
                OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Settings = new Dictionary<string, object>
                {
                    ["theme"] = "dark",
                    ["notifications"] = true,
                    ["autoSave"] = false
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await _context.Workspaces.AddRangeAsync(workspaces);
        _logger.LogInformation("Seeded {Count} workspaces", workspaces.Count);
    }

    private async Task SeedProjectsAsync()
    {
        _logger.LogInformation("Seeding projects...");

        var projects = new List<Project>
        {
            new Project
            {
                Id = Guid.Parse("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC"),
                Name = "Sample Project",
                Description = "A sample project to demonstrate workflow capabilities",
                IsActive = true,
                WorkspaceId = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Color = "#3498db",
                Settings = new Dictionary<string, object>
                {
                    ["enableVersioning"] = true,
                    ["requireApproval"] = false
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Project
            {
                Id = Guid.Parse("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD"),
                Name = "API Integration Hub",
                Description = "Central hub for managing API integrations across different services",
                IsActive = true,
                WorkspaceId = Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
                OwnerId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Color = "#e74c3c",
                Settings = new Dictionary<string, object>
                {
                    ["enableVersioning"] = true,
                    ["requireApproval"] = true
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await _context.Projects.AddRangeAsync(projects);
        _logger.LogInformation("Seeded {Count} projects", projects.Count);
    }

    private async Task SeedEnvironmentsAsync()
    {
        _logger.LogInformation("Seeding environments...");

        var environments = new List<Core.Entities.WorkflowEnvironment>
        {
            new Core.Entities.WorkflowEnvironment
            {
                Id = Guid.Parse("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE"),
                Name = "Development",
                Description = "Development environment for testing workflows",
                IsActive = true,
                ProjectId = Guid.Parse("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC"),
                Color = "#2ecc71",
                Variables = new Dictionary<string, string>
                {
                    ["API_BASE_URL"] = "https://api-dev.example.com",
                    ["TIMEOUT_SECONDS"] = "30",
                    ["RETRY_COUNT"] = "3"
                },
                Settings = new Dictionary<string, object>
                {
                    ["enableDebugMode"] = true,
                    ["logLevel"] = "Debug"
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Core.Entities.WorkflowEnvironment
            {
                Id = Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"),
                Name = "Production",
                Description = "Production environment for live workflows",
                IsActive = true,
                ProjectId = Guid.Parse("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC"),
                Color = "#e67e22",
                Variables = new Dictionary<string, string>
                {
                    ["API_BASE_URL"] = "https://api.example.com",
                    ["TIMEOUT_SECONDS"] = "60",
                    ["RETRY_COUNT"] = "5"
                },
                Settings = new Dictionary<string, object>
                {
                    ["enableDebugMode"] = false,
                    ["logLevel"] = "Information"
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Core.Entities.WorkflowEnvironment
            {
                Id = Guid.Parse("10101010-1010-1010-1010-101010101010"),
                Name = "Staging",
                Description = "Staging environment for integration testing",
                IsActive = true,
                ProjectId = Guid.Parse("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD"),
                Color = "#9b59b6",
                Variables = new Dictionary<string, string>
                {
                    ["API_BASE_URL"] = "https://api-staging.example.com",
                    ["TIMEOUT_SECONDS"] = "45",
                    ["RETRY_COUNT"] = "3"
                },
                Settings = new Dictionary<string, object>
                {
                    ["enableDebugMode"] = true,
                    ["logLevel"] = "Information"
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await _context.Environments.AddRangeAsync(environments);
        _logger.LogInformation("Seeded {Count} environments", environments.Count);
    }

    private async Task SeedApiEndpointsAsync()
    {
        _logger.LogInformation("Seeding API endpoints...");

        var apiEndpoints = new List<ApiEndpoint>
        {
            new ApiEndpoint
            {
                Id = Guid.Parse("12121212-1212-1212-1212-121212121212"),
                Name = "Get User Profile",
                Description = "Retrieves user profile information from the user service",
                Url = "https://jsonplaceholder.typicode.com/users/{userId}",
                Method = HttpMethod.GET,
                Type = ApiEndpointType.REST,
                RequestContentType = "application/json",
                ResponseContentType = "application/json",
                TimeoutSeconds = 30,
                IsActive = true,
                Headers = new Dictionary<string, string>
                {
                    ["Accept"] = "application/json",
                    ["User-Agent"] = "WorkflowManagement/1.0"
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System",
                Parameters = new List<ApiParameter>
                {
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "userId",
                        Description = "The ID of the user to retrieve",
                        Type = ParameterType.Integer,
                        IsRequired = true,
                        IsInput = true,
                        IsOutput = false,
                        Location = "path",
                        DefaultValue = "1",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    }
                }
            },
            new ApiEndpoint
            {
                Id = Guid.Parse("13131313-1313-1313-1313-131313131313"),
                Name = "Create Post",
                Description = "Creates a new post in the system",
                Url = "https://jsonplaceholder.typicode.com/posts",
                Method = HttpMethod.POST,
                Type = ApiEndpointType.REST,
                RequestContentType = "application/json",
                ResponseContentType = "application/json",
                TimeoutSeconds = 30,
                IsActive = true,
                Headers = new Dictionary<string, string>
                {
                    ["Accept"] = "application/json",
                    ["Content-Type"] = "application/json"
                },
                RequestBody = @"{
                    ""title"": ""{title}"",
                    ""body"": ""{body}"",
                    ""userId"": {userId}
                }",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System",
                Parameters = new List<ApiParameter>
                {
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "title",
                        Description = "The title of the post",
                        Type = ParameterType.String,
                        IsRequired = true,
                        IsInput = true,
                        IsOutput = false,
                        Location = "body",
                        DefaultValue = "Sample Post Title",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    },
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "body",
                        Description = "The content of the post",
                        Type = ParameterType.String,
                        IsRequired = true,
                        IsInput = true,
                        IsOutput = false,
                        Location = "body",
                        DefaultValue = "This is a sample post body content.",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    },
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "userId",
                        Description = "The ID of the user creating the post",
                        Type = ParameterType.Integer,
                        IsRequired = true,
                        IsInput = true,
                        IsOutput = false,
                        Location = "body",
                        DefaultValue = "1",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    }
                }
            },
            new ApiEndpoint
            {
                Id = Guid.Parse("14141414-1414-1414-1414-141414141414"),
                Name = "Send Email Notification",
                Description = "Sends an email notification via the notification service",
                Url = "https://httpbin.org/post",
                Method = HttpMethod.POST,
                Type = ApiEndpointType.REST,
                RequestContentType = "application/json",
                ResponseContentType = "application/json",
                TimeoutSeconds = 45,
                IsActive = true,
                Headers = new Dictionary<string, string>
                {
                    ["Accept"] = "application/json",
                    ["Content-Type"] = "application/json"
                },
                RequestBody = @"{
                    ""to"": ""{emailAddress}"",
                    ""subject"": ""{subject}"",
                    ""message"": ""{message}""
                }",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System",
                Parameters = new List<ApiParameter>
                {
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "emailAddress",
                        Description = "The email address to send notification to",
                        Type = ParameterType.String,
                        IsRequired = true,
                        IsInput = true,
                        IsOutput = false,
                        Location = "body",
                        DefaultValue = "user@example.com",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    },
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "subject",
                        Description = "The subject of the email",
                        Type = ParameterType.String,
                        IsRequired = true,
                        IsInput = true,
                        IsOutput = false,
                        Location = "body",
                        DefaultValue = "Workflow Notification",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    },
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "message",
                        Description = "The message content",
                        Type = ParameterType.String,
                        IsRequired = true,
                        IsInput = true,
                        IsOutput = false,
                        Location = "body",
                        DefaultValue = "Your workflow has completed successfully.",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    }
                }
            }
        };

        await _context.ApiEndpoints.AddRangeAsync(apiEndpoints);
        _logger.LogInformation("Seeded {Count} API endpoints", apiEndpoints.Count);
    }

    private async Task SeedWorkflowsAsync()
    {
        _logger.LogInformation("Seeding workflows...");

        var workflows = new List<Workflow>
        {
            new Workflow
            {
                Id = Guid.Parse("15151515-1515-1515-1515-151515151515"),
                Name = "User Onboarding Workflow",
                Description = "Automated workflow for new user onboarding process",
                Status = WorkflowStatus.Active,
                EnvironmentId = Guid.Parse("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE"),
                OwnerId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Tags = "onboarding,user,automation",
                TimeoutMinutes = 30,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-5),
                PublishedBy = "33333333-3333-3333-3333-333333333333",
                RetryCount = 2,
                GlobalVariables = new Dictionary<string, object>
                {
                    ["defaultUserId"] = 1,
                    ["notificationEmail"] = "admin@example.com"
                },
                Configuration = new Dictionary<string, object>
                {
                    ["enableNotifications"] = true,
                    ["logDetailLevel"] = "Verbose"
                },
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Workflow
            {
                Id = Guid.Parse("16161616-1616-1616-1616-161616161616"),
                Name = "Content Publishing Pipeline",
                Description = "Workflow for automated content creation and publishing",
                Status = WorkflowStatus.Draft,
                EnvironmentId = Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"),
                OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Tags = "content,publishing,automation",
                TimeoutMinutes = 60,
                IsPublished = false,
                RetryCount = 3,
                GlobalVariables = new Dictionary<string, object>
                {
                    ["publisherId"] = 2,
                    ["contentType"] = "article"
                },
                Configuration = new Dictionary<string, object>
                {
                    ["enableNotifications"] = false,
                    ["logDetailLevel"] = "Standard"
                },
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await _context.Workflows.AddRangeAsync(workflows);
        _logger.LogInformation("Seeded {Count} workflows", workflows.Count);
    }

    private async Task SeedWorkflowStepsAsync()
    {
        _logger.LogInformation("Seeding workflow steps...");

        var workflowSteps = new List<WorkflowStep>
        {
            new WorkflowStep
            {
                Id = Guid.Parse("17171717-1717-1717-1717-171717171717"),
                Name = "Get User Information",
                Description = "Retrieve user profile information for onboarding",
                WorkflowId = Guid.Parse("15151515-1515-1515-1515-151515151515"),
                ApiEndpointId = Guid.Parse("12121212-1212-1212-1212-121212121212"),
                Order = 1,
                IsActive = true,
                ContinueOnError = false,
                TimeoutSeconds = 30,
                RetryCount = 2,
                RetryDelaySeconds = 5,
                InputMapping = new Dictionary<string, object>
                {
                    ["userId"] = "{{global.defaultUserId}}"
                },
                OutputMapping = new Dictionary<string, object>
                {
                    ["userEmail"] = "{{response.email}}",
                    ["userName"] = "{{response.name}}"
                },
                Configuration = new Dictionary<string, object>
                {
                    ["validateResponse"] = true
                },
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-7),
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new WorkflowStep
            {
                Id = Guid.Parse("18181818-1818-1818-1818-181818181818"),
                Name = "Send Welcome Email",
                Description = "Send welcome email to the new user",
                WorkflowId = Guid.Parse("15151515-1515-1515-1515-151515151515"),
                ApiEndpointId = Guid.Parse("14141414-1414-1414-1414-141414141414"),
                Order = 2,
                IsActive = true,
                ContinueOnError = true,
                TimeoutSeconds = 45,
                RetryCount = 3,
                RetryDelaySeconds = 10,
                InputMapping = new Dictionary<string, object>
                {
                    ["emailAddress"] = "{{step1.userEmail}}",
                    ["subject"] = "Welcome to our platform!",
                    ["message"] = "Hello {{step1.userName}}, welcome to our platform!"
                },
                OutputMapping = new Dictionary<string, object>
                {
                    ["emailSent"] = "{{response.success}}",
                    ["messageId"] = "{{response.id}}"
                },
                Configuration = new Dictionary<string, object>
                {
                    ["allowFailure"] = true
                },
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-7),
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await _context.WorkflowSteps.AddRangeAsync(workflowSteps);
        _logger.LogInformation("Seeded {Count} workflow steps", workflowSteps.Count);
    }

    private async Task SeedSampleExecutionsAsync()
    {
        _logger.LogInformation("Seeding sample workflow executions...");

        var executions = new List<WorkflowExecution>
        {
            new WorkflowExecution
            {
                Id = Guid.Parse("19191919-1919-1919-1919-191919191919"),
                WorkflowId = Guid.Parse("15151515-1515-1515-1515-151515151515"),
                ExecutedById = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Status = ExecutionStatus.Completed,
                StartedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2),
                CompletedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2).AddMinutes(5),
                TotalSteps = 2,
                CompletedSteps = 2,
                FailedSteps = 0,
                TriggerType = "manual",
                TriggerSource = "web-ui",
                InputData = new Dictionary<string, object>
                {
                    ["userId"] = 1
                },
                OutputData = new Dictionary<string, object>
                {
                    ["userEmail"] = "user@example.com",
                    ["emailSent"] = true
                },
                Context = new Dictionary<string, object>
                {
                    ["environment"] = "development",
                    ["version"] = "1.0.0"
                },
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2).AddMinutes(5),
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new WorkflowExecution
            {
                Id = Guid.Parse("20202020-2020-2020-2020-202020202020"),
                WorkflowId = Guid.Parse("15151515-1515-1515-1515-151515151515"),
                ExecutedById = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Status = ExecutionStatus.Failed,
                StartedAt = DateTime.UtcNow.AddDays(-2).AddHours(-1),
                CompletedAt = DateTime.UtcNow.AddDays(-2).AddHours(-1).AddMinutes(3),
                ErrorMessage = "API endpoint timeout exceeded",
                TotalSteps = 2,
                CompletedSteps = 1,
                FailedSteps = 1,
                TriggerType = "scheduled",
                TriggerSource = "scheduler",
                InputData = new Dictionary<string, object>
                {
                    ["userId"] = 2
                },
                OutputData = new Dictionary<string, object>
                {
                    ["userEmail"] = "user2@example.com"
                },
                Context = new Dictionary<string, object>
                {
                    ["environment"] = "development",
                    ["version"] = "1.0.0"
                },
                CreatedAt = DateTime.UtcNow.AddDays(-2).AddHours(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-2).AddHours(-1).AddMinutes(3),
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await _context.WorkflowExecutions.AddRangeAsync(executions);
        _logger.LogInformation("Seeded {Count} sample executions", executions.Count);
    }

    public async Task SeedProductionDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting production data seeding...");

            // Only seed essential production data
            if (!await _context.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin))
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = "admin@company.com",
                    Username = "admin",
                    PasswordHash = CryptographyHelper.HashPassword("ChangeMe@123!"),
                    Role = UserRole.SuperAdmin,
                    IsActive = true,
                    EmailConfirmed = true,
                    Department = "IT",
                    JobTitle = "System Administrator",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                };

                await _context.Users.AddAsync(adminUser);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created default admin user. Username: admin, Password: ChangeMe@123!");
                _logger.LogWarning("IMPORTANT: Please change the default admin password immediately after first login!");
            }

            _logger.LogInformation("Production data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during production data seeding");
            throw;
        }
    }

    public async Task CleanupAsync()
    {
        try
        {
            _logger.LogInformation("Starting database cleanup...");

            // This method can be used to clean up test/development data
            // Only use in development/testing environments
            if (await _context.Users.AnyAsync(u => u.CreatedBy == "System"))
            {
                _logger.LogInformation("Removing seed data...");

                // Remove in reverse order due to foreign key constraints
                var seedExecutions = await _context.WorkflowExecutions
                    .Where(e => e.CreatedBy == "System")
                    .ToListAsync();
                _context.WorkflowExecutions.RemoveRange(seedExecutions);

                var seedSteps = await _context.WorkflowSteps
                    .Where(s => s.CreatedBy == "System")
                    .ToListAsync();
                _context.WorkflowSteps.RemoveRange(seedSteps);

                var seedWorkflows = await _context.Workflows
                    .Where(w => w.CreatedBy == "System")
                    .ToListAsync();
                _context.Workflows.RemoveRange(seedWorkflows);

                var seedEndpoints = await _context.ApiEndpoints
                    .Where(a => a.CreatedBy == "System")
                    .ToListAsync();
                _context.ApiEndpoints.RemoveRange(seedEndpoints);

                var seedEnvironments = await _context.Environments
                    .Where(e => e.CreatedBy == "System")
                    .ToListAsync();
                _context.Environments.RemoveRange(seedEnvironments);

                var seedProjects = await _context.Projects
                    .Where(p => p.CreatedBy == "System")
                    .ToListAsync();
                _context.Projects.RemoveRange(seedProjects);

                var seedWorkspaces = await _context.Workspaces
                    .Where(w => w.CreatedBy == "System")
                    .ToListAsync();
                _context.Workspaces.RemoveRange(seedWorkspaces);

                var seedUsers = await _context.Users
                    .Where(u => u.CreatedBy == "System")
                    .ToListAsync();
                _context.Users.RemoveRange(seedUsers);

                await _context.SaveChangesAsync();
                _logger.LogInformation("Seed data cleanup completed successfully.");
            }
            else
            {
                _logger.LogInformation("No seed data found to cleanup.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database cleanup");
            throw;
        }
    }

    // Helper method to check if running in development environment
    private bool IsDevelopmentEnvironment()
    {
        var environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
    }

    // Helper method to generate sample API endpoints for different types
    public async Task SeedAdditionalApiEndpointsAsync()
    {
        _logger.LogInformation("Seeding additional API endpoints...");

        var additionalEndpoints = new List<ApiEndpoint>
        {
            // REST API Examples
            new ApiEndpoint
            {
                Id = Guid.NewGuid(),
                Name = "Get Weather Data",
                Description = "Retrieves current weather information",
                Url = "https://api.openweathermap.org/data/2.5/weather",
                Method = HttpMethod.GET,
                Type = ApiEndpointType.REST,
                RequestContentType = "application/json",
                ResponseContentType = "application/json",
                TimeoutSeconds = 30,
                IsActive = true,
                RequiresAuthentication = true,
                Headers = new Dictionary<string, string>
                {
                    ["Accept"] = "application/json"
                },
                Authentication = new Dictionary<string, object>
                {
                    ["type"] = "apikey",
                    ["location"] = "query",
                    ["name"] = "appid"
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System",
                Parameters = new List<ApiParameter>
                {
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "q",
                        Description = "City name, state code and country code divided by comma",
                        Type = ParameterType.String,
                        IsRequired = true,
                        IsInput = true,
                        Location = "query",
                        DefaultValue = "London,UK",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    },
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "units",
                        Description = "Units of measurement",
                        Type = ParameterType.String,
                        IsRequired = false,
                        IsInput = true,
                        Location = "query",
                        DefaultValue = "metric",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    }
                }
            },

            // File Upload Example
            new ApiEndpoint
            {
                Id = Guid.NewGuid(),
                Name = "Upload Document",
                Description = "Uploads a document to the file storage service",
                Url = "https://httpbin.org/post",
                Method = HttpMethod.POST,
                Type = ApiEndpointType.REST,
                RequestContentType = "multipart/form-data",
                ResponseContentType = "application/json",
                TimeoutSeconds = 120,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System",
                Parameters = new List<ApiParameter>
                {
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "file",
                        Description = "The file to upload",
                        Type = ParameterType.File,
                        IsRequired = true,
                        IsInput = true,
                        Location = "form",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    },
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "description",
                        Description = "Description of the uploaded file",
                        Type = ParameterType.String,
                        IsRequired = false,
                        IsInput = true,
                        Location = "form",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    }
                }
            },

            // Database Query Example
            new ApiEndpoint
            {
                Id = Guid.NewGuid(),
                Name = "Search Products",
                Description = "Searches for products in the catalog",
                Url = "https://fakestoreapi.com/products",
                Method = HttpMethod.GET,
                Type = ApiEndpointType.REST,
                RequestContentType = "application/json",
                ResponseContentType = "application/json",
                TimeoutSeconds = 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System",
                Parameters = new List<ApiParameter>
                {
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "category",
                        Description = "Product category to filter by",
                        Type = ParameterType.String,
                        IsRequired = false,
                        IsInput = true,
                        Location = "query",
                        DefaultValue = "electronics",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    },
                    new ApiParameter
                    {
                        Id = Guid.NewGuid(),
                        Name = "limit",
                        Description = "Maximum number of products to return",
                        Type = ParameterType.Integer,
                        IsRequired = false,
                        IsInput = true,
                        Location = "query",
                        DefaultValue = "10",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    }
                }
            }
        };

        await _context.ApiEndpoints.AddRangeAsync(additionalEndpoints);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded {Count} additional API endpoints", additionalEndpoints.Count);
    }

    // Method to seed workflow schedules
    public async Task SeedWorkflowSchedulesAsync()
    {
        _logger.LogInformation("Seeding workflow schedules...");

        if (await _context.WorkflowSchedules.AnyAsync())
        {
            _logger.LogInformation("Workflow schedules already exist. Skipping seeding.");
            return;
        }

        var schedules = new List<WorkflowSchedule>
        {
            new WorkflowSchedule
            {
                Id = Guid.NewGuid(),
                WorkflowId = Guid.Parse("15151515-1515-1515-1515-151515151515"),
                Name = "Daily User Sync",
                Description = "Synchronizes user data daily at 2 AM",
                IsActive = true,
                Frequency = ScheduleFrequency.Daily,
                CronExpression = "0 0 2 * * ?", // Daily at 2 AM
                StartDate = DateTime.UtcNow.Date,
                NextRunTime = DateTime.UtcNow.Date.AddDays(1).AddHours(2),
                TimeZone = TimeZoneInfo.Utc,
                Parameters = new Dictionary<string, object>
                {
                    ["batchSize"] = 100,
                    ["enableNotifications"] = true
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new WorkflowSchedule
            {
                Id = Guid.NewGuid(),
                WorkflowId = Guid.Parse("15151515-1515-1515-1515-151515151515"),
                Name = "Weekly Report Generation",
                Description = "Generates weekly reports every Monday at 9 AM",
                IsActive = true,
                Frequency = ScheduleFrequency.Weekly,
                CronExpression = "0 0 9 ? * MON", // Every Monday at 9 AM
                StartDate = DateTime.UtcNow.Date,
                NextRunTime = GetNextMonday().AddHours(9),
                TimeZone = TimeZoneInfo.Utc,
                Parameters = new Dictionary<string, object>
                {
                    ["reportType"] = "weekly",
                    ["emailRecipients"] = new[] { "manager@example.com", "admin@example.com" }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        await _context.WorkflowSchedules.AddRangeAsync(schedules);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} workflow schedules", schedules.Count);
    }

    private DateTime GetNextMonday()
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0 && DateTime.UtcNow.Hour >= 9)
        {
            daysUntilMonday = 7; // If it's Monday after 9 AM, schedule for next Monday
        }
        return today.AddDays(daysUntilMonday);
    }
}