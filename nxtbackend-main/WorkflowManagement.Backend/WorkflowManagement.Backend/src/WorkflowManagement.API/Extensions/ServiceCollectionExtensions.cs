using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Quartz;
using System.Reflection;
using WorkflowManagement.Application.Mappings;
using WorkflowManagement.Application.Services;
using WorkflowManagement.Application.Validators;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Infrastructure.BackgroundServices;
using WorkflowManagement.Infrastructure.Caching;
using WorkflowManagement.Infrastructure.ExternalServices;
using WorkflowManagement.Infrastructure.Logging;
using WorkflowManagement.Infrastructure.Repositories;

namespace WorkflowManagement.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(AutoMapperProfile));

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AutoMapperProfile).Assembly));

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(CreateUserValidator).Assembly);

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IApiEndpointService, ApiEndpointService>();
        services.AddScoped<IWorkflowExecutionService, WorkflowExecutionService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<ISchedulingService, SchedulingService>();
        services.AddScoped<ISwaggerParserService, SwaggerParserService>();
        services.AddScoped<ISoapParserService, SoapParserService>();
        services.AddScoped<IMetricsService, MetricsService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Repository Pattern
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();
        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        services.AddScoped<IApiEndpointRepository, ApiEndpointRepository>();
        services.AddScoped<IWorkflowExecutionRepository, WorkflowExecutionRepository>();
        services.AddScoped<IApprovalRepository, ApprovalRepository>();
        services.AddScoped<IMetricsRepository, MetricsRepository>();

        // External Services
        services.AddHttpClient<HttpClientService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "WorkflowManagement/1.0");
        });

        services.AddScoped<SoapClientService>();
        services.AddScoped<EmailService>();

        // Caching
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "WorkflowManagement";
            });
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();
        }

        // Logging
        services.AddScoped<ILoggerService, SerilogService>();

        // Data Seeding
        services.AddScoped<WorkflowManagement.Infrastructure.Data.Seed.DataSeeder>();

        // Background Services with Quartz
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            // Workflow Scheduler Job
            var workflowSchedulerJobKey = new JobKey("WorkflowSchedulerJob");
            q.AddJob<WorkflowSchedulerService>(opts => opts.WithIdentity(workflowSchedulerJobKey));
            q.AddTrigger(opts => opts
                .ForJob(workflowSchedulerJobKey)
                .WithIdentity("WorkflowSchedulerJob-trigger")
                .WithCronSchedule("0 * * ? * *")); // Every minute

            // Metrics Collection Job
            var metricsJobKey = new JobKey("MetricsCollectionJob");
            q.AddJob<MetricsCollectionService>(opts => opts.WithIdentity(metricsJobKey));
            q.AddTrigger(opts => opts
                .ForJob(metricsJobKey)
                .WithIdentity("MetricsCollectionJob-trigger")
                .WithCronSchedule("0 0 1 * * ?")); // Daily at 1 AM

            // Cleanup Job
            var cleanupJobKey = new JobKey("CleanupJob");
            q.AddJob<CleanupService>(opts => opts.WithIdentity(cleanupJobKey));
            q.AddTrigger(opts => opts
                .ForJob(cleanupJobKey)
                .WithIdentity("CleanupJob-trigger")
                .WithCronSchedule("0 0 2 * * ?")); // Daily at 2 AM
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}

// MemoryCacheService.cs (for when Redis is not available)
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkflowManagement.Infrastructure.Caching;

namespace WorkflowManagement.API.Extensions;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache.TryGetValue(key, out var value))
            {
                if (value is string jsonValue)
                {
                    return JsonSerializer.Deserialize<T>(jsonValue);
                }
                return (T?)value;
            }
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.SetAbsoluteExpiration(expiration.Value);
            }

            var jsonValue = JsonSerializer.Serialize(value);
            _cache.Set(key, jsonValue, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _cache.Remove(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return _cache.TryGetValue(key, out _);
    }
}